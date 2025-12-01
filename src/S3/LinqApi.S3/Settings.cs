
namespace LinqApi.S3
{
    public class AwsSettings
    {

        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }

        public SqsSettings Sqs { get; set; } = new();
        public S3UploadSettings S3Upload { get; set; } = new();

        public LocationSettings Location { get; set; }


    }

    public class LocationSettings
    {
        public string PlaceIndexName { get; set; }
    }
    public class SqsSettings
    {
        public string QueueName { get; set; }
    }

    public class S3UploadSettings
    {
        public string BucketName { get; set; }

        public string UploadBucketName { get; set; }
        public string UploadPrefix { get; set; }
        public int UrlExpirationMinutes { get; set; }
        public string DefaultSiteId { get; set; }
        public string CdnBaseUrl { get; set; }

        public string ProcessorBucketName { get; set; }
        public string ProcessorUploadPrefix { get; set; }

        public string UploadQueueName { get; set; }
        public string ProcessQueueName { get; set; }

        public bool AiCheckEnabled { get; set; }

        public float AiCheckScoreThreshold { get; set; }
        public string AiQueueName { get; set; }
    }
}
