using MeetMindAI.Application.Common.Abstractions.Storage;
using MeetMindAI.Application.Common.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace MeetMindAI.Infrastructure.Storage.Local;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<LocalStorageOptions> options)
    {
        _rootPath = options.Value.RootPath;

        if (string.IsNullOrWhiteSpace(_rootPath))
            throw new InvalidOperationException("Local storage root path is not configured.");

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<FileStorageResult> SaveFileAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        if (file.Length == 0)
            throw new InvalidOperationException("Cannot save an empty file.");

        var extension = Path.GetExtension(file.FileName);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";

        var filePath = Path.Combine(_rootPath, storedFileName);

        var provider = new FileExtensionContentTypeProvider();

        // Use the stored file name to determine the content type
        if (!provider.TryGetContentType(storedFileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        await using var stream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await file.CopyToAsync(stream, cancellationToken);

        return new FileStorageResult
        {
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            StorageKey = storedFileName,
            ContentType = file.ContentType,
            Extension = extension,
            SizeInBytes = file.Length
        };
    }

    public Task<FileDownloadResult?> OpenReadAsync(
      string storageKey,
      CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_rootPath, storageKey);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<FileDownloadResult?>(null);
        }

        var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        var result = new FileDownloadResult
        {
            Stream = stream,
            FileName = storageKey,
            ContentType = "application/octet-stream"
        };

        return Task.FromResult<FileDownloadResult?>(result);
    }

    public Task DeleteFileAsync(
       string storageKey,
       CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_rootPath, storageKey);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
