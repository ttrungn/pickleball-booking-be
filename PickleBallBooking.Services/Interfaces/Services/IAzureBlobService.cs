namespace PickleBallBooking.Services.Interfaces.Services;

public interface IAzureBlobService
{
    Task<string> UploadAsync(Stream fileStream, string blobName, string containerName);
    Task<Stream> DownloadAsync(string blobName, string containerName);
    Task DeleteAsync(string blobName, string containerName);
    Task<IEnumerable<string>> ListBlobsAsync(string containerName);
}
