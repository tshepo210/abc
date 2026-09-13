using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace abc.Services
{
    public interface IProductBlobService
    {
        Task<string?> UploadAsync(IFormFile file, string blobNamePrefix = "");
        /// <summary>
        /// Returns the public URI for a blob stored in the container.
        /// </summary>
        string GetBlobUri(string blobName);
        Task<bool> DeleteAsync(string blobName);
    }
}
