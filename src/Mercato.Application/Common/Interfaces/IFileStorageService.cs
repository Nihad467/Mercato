namespace Mercato.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}