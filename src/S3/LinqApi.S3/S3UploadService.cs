using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using LinqApi.Repository;
using LinqApi.S3;
using Microsoft.Extensions.Options;

namespace Cikboard.S3
{

    public interface IS3UploadService
    {
        Task<PresignedUploadResult> GeneratePresignedUploadAsync (string fileName, string? siteId = null, Guid? contentId = null);


    }
    public class S3UploadService : IS3UploadService
    {
        private readonly S3UploadSettings _s3Settings;
        private readonly AmazonS3Client _s3Client;
        private readonly ILinqRepository<MediaAsset, Guid> mediaAssetRepository;
        private readonly ILinqRepository<S3Content, long> s3ContentRepository;

        public S3UploadService (IOptions<AwsSettings> awsOptions, ILinqRepository<MediaAsset, Guid> mediaAssetRepository, ILinqRepository<S3Content, long> s3ContentRepository)
        {
            var aws = awsOptions.Value;

            _s3Settings = aws.S3Upload;

            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(aws.Region)
            };

            _s3Client = new AmazonS3Client(aws.AccessKey, aws.SecretKey, config);
            this.mediaAssetRepository = mediaAssetRepository;
            this.s3ContentRepository = s3ContentRepository;
        }

        public async Task<PresignedUploadResult> GeneratePresignedUploadAsync (
      string fileName,
      string? siteId,
      Guid? contentId
  )
        {
            // 1. assetId belirle
            var assetId = contentId ?? Guid.NewGuid();

            // 2. mime + key hazırla
            var mimeType = MimeTypes.GetMimeType(fileName);
            fileName = fileName.Replace("\\", "/");
            fileName = Path.GetFileName(fileName);

            var s3Key = $"uploads/{assetId}/{fileName}";

            // 3. MediaAsset var mı?
            // repos.MediaAssets gibi bir repo varsayalım:

            var mediaAsset = await mediaAssetRepository.GetByIdAsync(assetId);

            if (mediaAsset == null)
            {
                mediaAsset = new MediaAsset
                {
                    Id = assetId,
                    AssetType = null,                  // istersen burada set edebilirsin
                    Status = UploadStatus.Unknown,
                    // S3Contents boş başlar
                };

                await mediaAssetRepository.InsertAsync(mediaAsset);
                // SaveChangesAsync çağrısını InsertAsync içinde zaten yapıyorsan ekstra gerek yok.
            }

            // 4. S3Content insert et
            var s3Content = new S3Content
            {
                Key = s3Key,
                Bucket = _s3Settings.BucketName,
                MimeType = mimeType,
                MediaAssetId = assetId,
                Status = UploadStatus.Unknown
            };

            await s3ContentRepository.InsertAsync(s3Content);

            // 5. Presigned URL üret
            var uploadUrl = await _s3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = s3Key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(15),
                ContentType = mimeType
            });

            // 6. client'a dön
            return new PresignedUploadResult
            {
                UploadUrl = uploadUrl,
                MimeType = mimeType,
                ContentId = assetId,
                S3Key = s3Key
            };
        }
    }
}