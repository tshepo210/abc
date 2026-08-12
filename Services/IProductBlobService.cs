using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace abc.Services
{
    public interface IProductBlobService
    {
        Task<string?> UploadAsync(IFormFile file, string blobNamePrefix = "");
        Task<bool> DeleteAsync(string blobName);
    }
}
