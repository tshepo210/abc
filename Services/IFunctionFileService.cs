using Microsoft.AspNetCore.Http;

namespace abc.Services
{
    public interface IFunctionFileService
    {
        Task UploadAsync(IFormFile file, string fileName);
    }
}