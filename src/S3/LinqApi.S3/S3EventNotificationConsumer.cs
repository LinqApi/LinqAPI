using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;
using LinqApi.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using System.Text.Json;

namespace LinqApi.S3
{
    public class S3FileUploadedEventNotificationConsumer : IConsumer<S3UploadedEventNotification>
    {
        private readonly ILogger<S3FileUploadedEventNotificationConsumer> _logger;
        private readonly ILinqRepository<S3Content,long> s3ContentRepository;
        private readonly ILinqRepository<MediaAsset, Guid> mediaAssetRepository;
        private readonly AwsSettings awsOptions;

        public S3FileUploadedEventNotificationConsumer (
            ILogger<S3FileUploadedEventNotificationConsumer> logger,
            IOptions<AwsSettings> awsOptions,
            ILinqRepository<S3Content, long> s3ContentRepository,
            ILinqRepository<MediaAsset, Guid> mediaAssetRepository)
        {
            _logger = logger;
            this.awsOptions = awsOptions.Value;
            this.s3ContentRepository = s3ContentRepository;
            this.mediaAssetRepository = mediaAssetRepository;
        }

        public async Task Consume(ConsumeContext<S3UploadedEventNotification> context)
        {
            foreach (var record in context.Message.Records)
            {
                if (record is null) return;

                var bucket = record.S3.Bucket.Name;
                var key = record.S3.Object.Key;

                bool exists;
                S3Content content;
                MediaAsset mediaAsset;
                ILinqRepository<S3Content, long> repoValue = s3ContentRepository;
                (exists, content) = await repoValue.TryFindFastAsync(s => s.Key == key);

                ILinqRepository<MediaAsset, Guid> assetValue = mediaAssetRepository;
                (exists, mediaAsset) = await assetValue.TryFindFastAsync(s => s.Id == content.MediaAssetId);

                if (exists)
                {
                    content.Status = UploadStatus.Uploaded;
                    mediaAsset.Status = UploadStatus.Uploaded;
                    content.MediaType = DetermineMediaTypeFromExtension(Path.GetExtension(key));
                    await repoValue.UpdateAsync(content);
                    await assetValue.UpdateAsync(mediaAsset);

                    if (awsOptions.S3Upload.AiCheckEnabled)
                    {
                        await context.Publish(new S3FileCheckAiNotification { Entity = content });
                    }
                    else
                    {
                        await context.Publish(new S3FileResizeNotification { Entity = content });
                    }
                }

                _logger.LogInformation("S3 content saved: {Key}", key);
            }

        }

        private MediaType DetermineMediaTypeFromExtension(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                case ".webp":
                    return MediaType.Image;

                case ".gif":
                    return MediaType.Gif;

                case ".mp4":
                case ".mov":
                case ".avi":
                case ".mkv":
                case ".webm":
                    return MediaType.Video;

                default:
                    return MediaType.Unknown;
            }
        }

    }
    public class S3FileCheckAiNotificationConsumer : IConsumer<S3FileCheckAiNotification>
    {
        private readonly ILinqRepository<S3Content, long> s3ContentRepository;
        private readonly ILinqRepository<S3ContentAiResult, long> s3ContentAiResultsRepository;
        private readonly ILinqRepository<MediaAsset, Guid> mediaAssetRepository;
        private readonly ILogger<S3FileCheckAiNotificationConsumer> _logger;
        private readonly AwsSettings _awsOptions;
        private readonly IAmazonRekognition _rekognition;
        private static readonly HashSet<string> BlockedCategories = new()
{
    "Explicit Nudity",
    "Non-Explicit Nudity of Intimate parts and Kissing",
    "Violence",
    "Visually Disturbing",
    "Drugs",
    "Weapons",
    "Hate Symbols",
    "Alcohol" // isteğe bağlı
};
        public S3FileCheckAiNotificationConsumer (
            ILogger<S3FileCheckAiNotificationConsumer> logger,
            IOptions<AwsSettings> awsOptions,
            IAmazonRekognition rekognition,
            ILinqRepository<S3Content, long> s3ContentRepository,
            ILinqRepository<MediaAsset, Guid> mediaAssetRepository,
            ILinqRepository<S3ContentAiResult, long> s3ContentAiResultsRepository)
        {
            _logger = logger;
            _awsOptions = awsOptions.Value;
            var credentials = new BasicAWSCredentials(_awsOptions.AccessKey, _awsOptions.SecretKey);
            var region = RegionEndpoint.GetBySystemName(_awsOptions.Region);
            _rekognition = new AmazonRekognitionClient(credentials, region);
            this.s3ContentRepository = s3ContentRepository;
            this.mediaAssetRepository = mediaAssetRepository;
            this.s3ContentAiResultsRepository = s3ContentAiResultsRepository;
        }


        public async Task Consume(ConsumeContext<S3FileCheckAiNotification> context)
        {
            var bucket = context.Message.Entity.Bucket;
            var key = context.Message.Entity.Key;

            // 4) DB kaydını bul
            var repoValue = s3ContentRepository;
            var content = context.Message.Entity;

            // 2) Rekognition ile moderation labels
            var detectRequest = new DetectModerationLabelsRequest
            {
                Image = new Amazon.Rekognition.Model.Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = bucket,
                        Name = key
                    }
                },
                MinConfidence = (float)_awsOptions.S3Upload.AiCheckScoreThreshold
            };

            DetectModerationLabelsResponse detectResponse;
            try
            {
                detectResponse = await _rekognition.DetectModerationLabelsAsync(detectRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rekognition call failed for {Key}", key);
                throw;
            }

            // 3) Threshold ve flagli etiket kontrolü
            var flaggedLabels = detectResponse.ModerationLabels
                .Where(label =>
                    label.Confidence >= _awsOptions.S3Upload.AiCheckScoreThreshold &&
                    (BlockedCategories.Contains(label.Name) || (BlockedCategories.Contains(label.ParentName) || BlockedCategories.Contains(label.Name))))
                .ToList();

            bool hasHighLabel = flaggedLabels.Any();

            var newStatus = hasHighLabel
                ? UploadStatus.AiRejected
                : UploadStatus.AiApproved;

            _logger.LogInformation(
                "AI check for {Key}: {Result} (threshold: {Threshold})",
                key,
                hasHighLabel ? "Rejected" : "Approved",
                _awsOptions.S3Upload.AiCheckScoreThreshold
            );

            // status güncelle
            content.Status = newStatus;
            await repoValue.UpdateAsync(content);

            // 5) AI sonucu kaydet
            var aiResult = new S3ContentAiResult
            {
                S3ContentId = content.Id,
                AiProvider = "Linq Rekognition",
                Threshold = _awsOptions.S3Upload.AiCheckScoreThreshold,
                HasFlaggedLabels = hasHighLabel,
                FlaggedLabelCount = flaggedLabels.Count,
                RawResultJson = JsonSerializer.Serialize(detectResponse),
            };

            await s3ContentAiResultsRepository.InsertAsync(aiResult);

            if (newStatus == UploadStatus.AiApproved)
            {
                await context.Publish(new S3FileCheckProcessNotification { Entity = content });
            }
        }
    }
}
