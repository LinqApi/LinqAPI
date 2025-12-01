using Microsoft.Extensions.Options;

namespace LinqApi.S3
{
    public interface IS3UrlGenerator
    {
        string GetPublicUrl(string contentKey);
    }

    public class S3UrlGenerator : IS3UrlGenerator
    {
        private readonly AwsSettings _aws;

        public S3UrlGenerator(IOptions<AwsSettings> options)
        {
            _aws = options.Value;
        }

        public string GetPublicUrl(string contentKey)
        {
           

            // CDN varsa onu kullan, yoksa direkt S3 public URL üret
            if (!string.IsNullOrWhiteSpace(_aws.S3Upload.CdnBaseUrl))
            {
                return $"{_aws.S3Upload.CdnBaseUrl.TrimEnd('/')}/{contentKey}";
            }

            // AWS'nin public URL formatı
            return $"https://{_aws.S3Upload.BucketName}.s3.amazonaws.com/{contentKey}";
        }
    }

}
