using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LinqApi.S3
{
    public interface IImageService
    {
        Task<(int Width, int Height)> FetchImageMetadataAsync(string bucket, string key);
        Task<S3Content> ProcessAndStoreImageAsync(
            S3Content original, S3ContentRole role, double scale);
        Task<S3Content> ProcessAndStoreGifAsync(
            S3Content original, S3ContentRole role, int maxSize);
    }

    public class ImageService : IImageService
    {
        private readonly IAmazonS3 _s3;
        private readonly S3UploadSettings _settings;
        private readonly ILogger<ImageService> _logger;

        public ImageService(
            IAmazonS3 s3,
            IOptions<AwsSettings> awsOpts,
            ILogger<ImageService> logger)
        {
            _s3 = s3;
            _settings = awsOpts.Value.S3Upload;
            _logger = logger;
        }

        public async Task<(int Width, int Height)> FetchImageMetadataAsync(
            string bucket, string key)
        {
            using var res = await _s3.GetObjectAsync(
                new GetObjectRequest { BucketName = bucket, Key = key });
            using var img = await Image.LoadAsync(res.ResponseStream);
            return (img.Width, img.Height);
        }

        public async Task<S3Content> ProcessAndStoreImageAsync(
            S3Content original, S3ContentRole role, double scale)
        {
            var tmpIn = Path.GetTempFileName();
            var tmpOut = Path.GetTempFileName();

            try
            {
                await DownloadToFileAsync(original.Bucket!, original.Key!, tmpIn);

                int newW, newH;
                using (var image = await Image.LoadAsync(tmpIn))
                {
                    newW = (int)(original.Width!.Value * scale);
                    newH = (int)(original.Height!.Value * scale);

                    image.Mutate(x => x.Resize(newW, newH));
                    await image.SaveAsync(tmpOut, new WebpEncoder());
                }

                var child = await UploadAndBuildContentAsync(
                    tmpOut, original, role, newW, newH);

                _logger.LogInformation("Uploaded {Role}: {Key}", role, child.Key);
                return child;
            }
            finally
            {
                TryDelete(tmpIn);
                TryDelete(tmpOut);
            }
        }

        public async Task<S3Content> ProcessAndStoreGifAsync(
            S3Content original, S3ContentRole role, int maxSize)
        {
            var tmpIn = Path.GetTempFileName();
            var tmpOut = Path.GetTempFileName();

            try
            {
                await DownloadToFileAsync(original.Bucket!, original.Key!, tmpIn);

                using (var image = await Image.LoadAsync(tmpIn)) // ilk kare
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxSize, maxSize)
                    }));
                    await image.SaveAsync(tmpOut, new WebpEncoder());
                }

                var child = await UploadAndBuildContentAsync(
                    tmpOut, original, role, maxSize, maxSize);

                _logger.LogInformation("Uploaded GIF thumbnail: {Key}", child.Key);
                return child;
            }
            finally
            {
                TryDelete(tmpIn);
                TryDelete(tmpOut);
            }
        }

        private async Task<S3Content> UploadAndBuildContentAsync(
            string filePath,
            S3Content original,
            S3ContentRole role,
            int width,
            int height)
        {
            var dir = Path.GetDirectoryName(original.Key)!.Replace("\\", "/").TrimEnd('/');
            var fileName = Path.GetFileNameWithoutExtension(original.Key);

            var outKey = $"{_settings.UploadPrefix}{dir}/{fileName}_{role.ToString().ToLower()}.webp";

            await _s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _settings.UploadBucketName,
                Key = outKey,
                FilePath = filePath,
                ContentType = "image/webp"
            });

            var fi = new FileInfo(filePath);
            return new S3Content
            {
                Bucket = _settings.UploadBucketName,
                Key = outKey,
                MimeType = "image/webp",
                SizeBytes = fi.Length,
                Width = width,
                Height = height,
                MediaType = MediaType.Image,
                Role = role,
                MediaAssetId = original.MediaAssetId,
                Status = UploadStatus.ReadyToUse
            };
        }

        private async Task DownloadToFileAsync(
            string bucket, string key, string destPath)
        {
            using var res = await _s3.GetObjectAsync(
                new GetObjectRequest { BucketName = bucket, Key = key });
            await using var fs = File.OpenWrite(destPath);
            await res.ResponseStream.CopyToAsync(fs);
        }

        private void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Temp file delete failed: {Path}", path);
            }
        }
    }
}
