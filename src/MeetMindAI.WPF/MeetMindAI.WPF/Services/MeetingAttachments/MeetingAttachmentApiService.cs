using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using MeetMindAI.WPF.Models.MeetingAttachments;

namespace MeetMindAI.WPF.Services.MeetingAttachments;

public sealed class MeetingAttachmentApiService
    : IMeetingAttachmentApiService
{
    private readonly HttpClient _httpClient;

    public MeetingAttachmentApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<MeetingAttachmentItem>> GetAllAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                $"api/meetings/{meetingId}/attachments",
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to load meeting attachments.",
                cancellationToken);
        }

        return await response.Content
                   .ReadFromJsonAsync<List<MeetingAttachmentItem>>(
                       cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<UploadAttachmentResponse> UploadAsync(
        Guid meetingId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "A file path is required.",
                nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "The selected file could not be found.",
                filePath);
        }

        await using var fileStream =
            File.OpenRead(filePath);

        using var fileContent =
            new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                GetContentType(filePath));

        using var form =
            new MultipartFormDataContent();

        form.Add(
            fileContent,
            "File",
            Path.GetFileName(filePath));

        using var response =
            await _httpClient.PostAsync(
                $"api/meetings/{meetingId}/attachments",
                form,
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to upload attachment.",
                cancellationToken);
        }

        return await response.Content
                   .ReadFromJsonAsync<UploadAttachmentResponse>(
                       cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "The server returned an empty upload response.");
    }

    public async Task<DownloadedAttachment> DownloadAsync(
        Guid meetingId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                $"api/meetings/{meetingId}/attachments/{attachmentId}",
                cancellationToken);

        EnsureAuthorized(response);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException(
                "The attachment could not be found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to download attachment.",
                cancellationToken);
        }

        var content =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        var contentType =
            response.Content.Headers.ContentType?.MediaType
            ?? "application/octet-stream";

        var fileName =
            response.Content.Headers.ContentDisposition?
                .FileNameStar
            ?? response.Content.Headers.ContentDisposition?
                .FileName
            ?? $"{attachmentId}";

        fileName = fileName.Trim('"');

        return new DownloadedAttachment(
            fileName,
            contentType,
            content);
    }

    public async Task DeleteAsync(
        Guid meetingId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.DeleteAsync(
                $"api/meetings/{meetingId}/attachments/{attachmentId}",
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to delete attachment.",
                cancellationToken);
        }
    }

    private static void EnsureAuthorized(
        HttpResponseMessage response)
    {
        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }
    }

    private static async Task<Exception> CreateExceptionAsync(
        HttpResponseMessage response,
        string message,
        CancellationToken cancellationToken)
    {
        var error =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        return new InvalidOperationException(
            string.IsNullOrWhiteSpace(error)
                ? message
                : $"{message} {error}");
    }

    private static string GetContentType(
        string filePath)
    {
        return Path.GetExtension(filePath)
            .ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",

            ".txt" => "text/plain",

            ".doc" => "application/msword",

            ".docx" =>
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            ".xls" => "application/vnd.ms-excel",

            ".xlsx" =>
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

            ".png" => "image/png",

            ".jpg" or ".jpeg" => "image/jpeg",

            ".mp3" => "audio/mpeg",

            ".wav" => "audio/wav",

            ".mp4" => "video/mp4",

            _ => "application/octet-stream"
        };
    }
}
