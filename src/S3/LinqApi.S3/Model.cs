using Amazon.Rekognition.Model;
using LinqApi.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LinqApi.S3;
public class S3UploadedEventNotification
{
    [JsonPropertyName("Records")]
    public List<S3Record>? Records { get; set; } = new();
}

public class S3FileCheckAiNotification
{
    public S3Content Entity { get; set; }
}

public class S3FileCheckProcessNotification
{
    public S3Content Entity { get; set; }
}

public class S3FileResizeNotification
{
    public S3Content Entity { get; set; }
}

public class S3Record
{
    [JsonPropertyName("eventVersion")]
    public string? EventVersion { get; set; }

    [JsonPropertyName("eventSource")]
    public string? EventSource { get; set; }

    [JsonPropertyName("awsRegion")]
    public string? AwsRegion { get; set; }

    [JsonPropertyName("eventTime")]
    public string? EventTime { get; set; }

    [JsonPropertyName("eventName")]
    public string? EventName { get; set; }

    [JsonPropertyName("userIdentity")]
    public UserIdentity? UserIdentity { get; set; }

    [JsonPropertyName("requestParameters")]
    public RequestParameters? RequestParameters { get; set; }

    [JsonPropertyName("responseElements")]
    public ResponseElements? ResponseElements { get; set; }

    [JsonPropertyName("s3")]
    public S3RecordS3? S3 { get; set; }
}
public class UserIdentity
{
    [JsonPropertyName("principalId")]
    public string? PrincipalId { get; set; }
}

public class RequestParameters
{
    [JsonPropertyName("sourceIPAddress")]
    public string? SourceIPAddress { get; set; }
}

public class ResponseElements
{
    [JsonPropertyName("x-amz-request-id")]
    public string? XAmzRequestId { get; set; }

    [JsonPropertyName("x-amz-id-2")]
    public string? XAmzId2 { get; set; }
}

public class S3RecordS3
{
    [JsonPropertyName("s3SchemaVersion")]
    public string? S3SchemaVersion { get; set; }

    [JsonPropertyName("configurationId")]
    public string? ConfigurationId { get; set; }

    [JsonPropertyName("bucket")]
    public S3Bucket? Bucket { get; set; }

    [JsonPropertyName("arn")]
    public string? Arn { get; set; }

    [JsonPropertyName("object")]
    public S3Object? Object { get; set; }
}

public class S3Bucket
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("ownerIdentity")]
    public OwnerIdentity? OwnerIdentity { get; set; }


}

public class OwnerIdentity
{
    [JsonPropertyName("principalId")]
    public string? PrincipalId { get; set; }
}

public class S3Object
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("eTag")]
    public string? ETag { get; set; }

    [JsonPropertyName("sequencer")]
    public string? Sequencer { get; set; }
}
/// <summary>
/// Backend tarafından dönen S3 upload bilgisi (Presigned URL ile birlikte)
/// </summary>
public class PresignedUploadResult
{
    public string UploadUrl { get; set; } = default!;  // PUT işlemi yapılacak URL
    public string S3Key { get; set; } = default!;      // S3 Key (veri işlemek için)
    public string MimeType { get; set; } = default!;   // Upload edilecek dosyanın tipi
    public string? SiteId { get; set; }                // Kaynağın ilişkili olduğu site (isteğe bağlı)

    // Ek bilgiler (İleride kullanılmak üzere)
    public string? Bucket { get; set; }                // S3 bucket (isteğe bağlı ama faydalı)
    public S3ContentRole Role { get; set; } = S3ContentRole.Original;  // Dosya rolü (default olarak Original)

    // Timestamp ve debug amaçlı faydalı olabilir
    public DateTime ExpirationTime { get; set; } = DateTime.Now.AddMinutes(15); // Presigned URL süresi
    public Guid ContentId { get; set; }
}

public enum MediaType { Unknown = 0, Image = 1, Gif = 2, Video = 3 }

public enum UploadStatus { Unknown = 0, Uploaded = 10, AiChecked = 20, AiRejected = 30, AiApproved = 40, ResizeStarted = 50, ResizeError = 60, ReadyToUse = 100 }



// Gerekli using'ları eklemeyi unutmayın.

public class S3Content : BaseEntity<long>
{
    // S3 Bilgileri
    [MaxLength(256)]
    public string? Bucket { get; set; }

    [MaxLength(1024)]
    public string? Key { get; set; }

    [MaxLength(128)]
    public string? MimeType { get; set; }

    // Dosya Özellikleri
    public long? SizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? DurationSeconds { get; set; }

    // Meta Veriler
    public MediaType? MediaType { get; set; } = S3.MediaType.Unknown;

    // ÖNEMLİ: Bu dosyanın gruptaki rolünü belirtir (Orijinal mi, Thumbnail mi vb.)
    public S3ContentRole? Role { get; set; } = S3ContentRole.Original;
    public UploadStatus? Status { get; set; } = UploadStatus.Unknown;


    public Guid? MediaAssetId { get; set; }

    [ForeignKey(nameof(MediaAssetId))]
    [JsonIgnore]
    public MediaAsset MediaAsset { get; set; }

    public ICollection<S3ContentAiResult> AiResults { get; set; } = new List<S3ContentAiResult>();
}

public class MediaAsset : BaseEntity<Guid> // ID için Guid kullanmak daha mantıklı olabilir.
{
    [MaxLength(128)]
    public string? AssetType { get; set; }

    public UploadStatus Status { get; set; }

    // Bu MediaAsset'e bağlı tüm S3 dosyaları (versiyonları).
    public ICollection<S3Content> S3Contents { get; set; } = new List<S3Content>();

    public S3Content Thumbnail => S3Contents.FirstOrDefault(s => s.Role == S3ContentRole.Thumbnail);
    public S3Content Small => S3Contents.FirstOrDefault(s => s.Role == S3ContentRole.Small);
    public S3Content Medium => S3Contents.FirstOrDefault(s => s.Role == S3ContentRole.Medium);
    public S3Content Large => S3Contents.FirstOrDefault(s => s.Role == S3ContentRole.Large);
}




public class S3ContentAiResult : BaseEntity<long>
{
    [ForeignKey(nameof(S3Content))]
    public long S3ContentId { get; set; }

    [JsonIgnore]
    public S3Content S3Content { get; set; } = null!;

    [MaxLength(128)]
    public string AiProvider { get; set; } = "AWS Rekognition";

    public float Threshold { get; set; }

    // moderation result JSON'ı
    public string? RawResultJson { get; set; }

    // özet bilgi
    public bool HasFlaggedLabels { get; set; }

    // flaglenmiş label sayısı
    public int FlaggedLabelCount { get; set; }

}

public enum S3ContentRole : short
{
    Original = 0,
    Thumbnail = 10,
    Small = 20,
    Medium = 30,
    Large = 40
}


