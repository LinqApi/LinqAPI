using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using LinqApi.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LinqApi.S3
{
    /// <summary>
    /// S3'e yüklenen her tür medya dosyasını işleyen temel orchestrator
    /// </summary>
    public class S3MediaProcessor : IConsumer<S3FileCheckProcessNotification>
    {
        private readonly IImageService _imageService;
        private readonly ILogger<S3MediaProcessor> _logger;
        private readonly ILinqRepository<MediaAsset, Guid> mediaAssetRepository;
        private readonly ILinqRepository<S3Content, long> s3ContentRepository;

        public S3MediaProcessor (
            IImageService imageService,
            ILogger<S3MediaProcessor> logger,
            ILinqRepository<MediaAsset, Guid> mediaAssetRepository,
            ILinqRepository<S3Content, long> s3ContentRepository)
        {
            _imageService = imageService;
            _logger = logger;
            this.mediaAssetRepository = mediaAssetRepository;
            this.s3ContentRepository = s3ContentRepository;
        }

        public async Task Consume (ConsumeContext<S3FileCheckProcessNotification> context)
        {
            var original = context.Message.Entity;
            bool exists;
            MediaAsset mediaAsset;

            (exists, mediaAsset) = await mediaAssetRepository.TryFindFastAsync(s => s.Id == original.MediaAssetId);
            if (original == null) return;

            // 1) Metadata güncelle
            var (width, height) = await _imageService.FetchImageMetadataAsync(
                original.Bucket!, original.Key!);
            original.Width = width;
            original.Height = height;
            await s3ContentRepository.UpdateAsync(original);

            // 2) İşleme: Image vs GIF
            if (original.MediaType == MediaType.Image)
            {
                var targets = new[]
                {
                new { Role = S3ContentRole.Thumbnail, MaxW = 150,  MaxH = 150 },
                new { Role = S3ContentRole.Small,     MaxW = 320,  MaxH = 240 },
                new { Role = S3ContentRole.Medium,    MaxW = 800,  MaxH = 600 },
                new { Role = S3ContentRole.Large,     MaxW = 1600, MaxH = 1200 }
            };

                foreach (var t in targets)
                {
                    var scale = Math.Min(
                        (double)t.MaxW / original.Width!.Value,
                        (double)t.MaxH / original.Height!.Value
                    );
                    scale = Math.Min(scale, 1.0);

                    var child = await _imageService.ProcessAndStoreImageAsync(
                        original, t.Role, scale
                    );

                    await s3ContentRepository.InsertAsync(child);
                }
            }
            else if (original.MediaType == MediaType.Gif)
            {
                // GIF için sadece thumbnail (küçük kare) üretelim
                var child = await _imageService.ProcessAndStoreGifAsync(
                    original,
                    role: S3ContentRole.Thumbnail,
                    maxSize: 320
                );
                await s3ContentRepository.InsertAsync(child);
            }

            // 3) Orijinali hazır duruma getir
            original.Status = UploadStatus.ReadyToUse;
            mediaAsset.Status = UploadStatus.ReadyToUse;
            await mediaAssetRepository.UpdateAsync(mediaAsset);
            await s3ContentRepository.UpdateAsync(original);

            _logger.LogInformation(
                "Media processing completed: {Key} ({Type})",
                original.Key, original.MediaType);
        }
    }

    /// <summary>
    /// Videolar için işlemci arayüzü
    /// </summary>
    public interface IVideoProcessor
    {
        Task ProcessVideoAsync (string bucket, string key, string mimeType);
    }

    /// <summary>
    /// GIF'ler için işlemcccccci arayüzü
    /// </summary>
    public interface IGifProcessor
    {
        Task ProcessGifAsync (string bucket, string key, string mimeType);
    }

    public class GifProcessor : IGifProcessor
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName = "your-bucket";

        public GifProcessor (IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task ProcessGifAsync (string bucket, string key, string mimeType)
        {
            // 🧪 1. S3'ten indir
            using var originalStream = new MemoryStream();
            var getRequest = new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(getRequest);
            await response.ResponseStream.CopyToAsync(originalStream);
            originalStream.Position = 0;
            originalStream.Position = 0;

            // 🎞 2. İlk kareyi oku (ImageSharp henüz animasyon desteklemiyor)
            using var image = await Image.LoadAsync(originalStream);

            // 📏 3. Oranlı şekilde thumbnail boyutlandır
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(320, 320) // max width/height
            }));

            // 💾 4. WebP'e dönüştür ve yeni stream'e yaz
            using var webpStream = new MemoryStream();
            await image.SaveAsync(webpStream, new WebpEncoder
            {
                Quality = 75,
                FileFormat = WebpFileFormatType.Lossy
            });

            webpStream.Position = 0;

            // 📤 5. S3'e upload
            var thumbnailKey = key.Replace(".gif", "_thumb.webp");

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = webpStream,
                Key = thumbnailKey,
                BucketName = bucket,
                ContentType = "image/webp"
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            // 🧩 6. Metadata kaydet (örnek: S3Content tablosuna ekleme)
            Console.WriteLine($"✔️ Thumbnail yüklendi: s3://{bucket}/{thumbnailKey}");
        }

        // Her bir processor implementasyonunda ilgili kütüphaneler (ImageSharp, FFmpeg, vs) kullanılabilir.
        // İşlem sonrası oluşturulan küçük boyutlu görseller tekrar S3'e yüklenebilir ve S3Content'e kaydedilebilir.
    }
}
