using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Mercato.Infrastructure.Services;

public class S3MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;

    public S3MinioFileStorageService(
        IMinioClient minioClient,
        IOptions<MinioOptions> options)
    {
        _minioClient = minioClient;
        _options = options.Value;
    }

    public async Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.BucketName),
            cancellationToken);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_options.BucketName),
                cancellationToken);
        }

        var objectKey = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        await _minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType),
            cancellationToken);

        return objectKey;
    }

    public async Task DeleteFileAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        await _minioClient.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey),
            cancellationToken);
    }
}