using Amazon.Rekognition;
using Amazon.S3;
using Cikboard.S3;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace LinqApi.S3;
public static class MimeTypes
{
    public static string GetMimeType (string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();
        return provider.TryGetContentType(fileName, out var mime) ? mime : "application/octet-stream";
    }

    public static void S3Requirements (IServiceCollection services)
    {
        services.AddScoped<IS3UploadService, S3UploadService>();
        services.AddSingleton<IS3UrlGenerator, S3UrlGenerator>();

        services.AddScoped<IGifProcessor, GifProcessor>();

        services.AddScoped<IImageService, ImageService>();

        services.AddAWSService<IAmazonRekognition>(); // AWS SDK extensio
        services.AddAWSService<IAmazonS3>();
       
    }
}