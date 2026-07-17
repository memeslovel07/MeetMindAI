using Microsoft.AspNetCore.Http;
using MeetMindAI.Application.Common.Models;

namespace MeetMindAI.Application.Common.Abstractions.Storage;

public interface IFileStorageService
{
    Task<FileStorageResult> SaveFileAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task<FileDownloadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
