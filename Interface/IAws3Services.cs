namespace CachingS3.Interface
{
    public interface IAws3Services
    {
        Task<byte[]> DownloadFileAsync(string file);

        Task<IEnumerable<string>> GetAllFileNames();

        Task<bool> UploadFileAsync(IFormFile file);

        // add two new services: createCacheByRequest
        // searchCache, Use a hash to generate a key for the archive, taking into account any elements you use in your project, such as CNPJ, filters, etc...
    }
}
