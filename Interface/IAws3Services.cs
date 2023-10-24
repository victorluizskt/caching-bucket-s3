namespace CachingS3.Interface
{
    public interface IAws3Services
    {
        Task<byte[]> DownloadFileAsync(string file);

        Task<IEnumerable<string>> GetAllFileNames();

        Task<bool> UploadFileAsync(IFormFile file);

    }
}
