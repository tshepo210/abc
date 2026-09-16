using System.Net.Http.Headers;
using abc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace abc.Services
{
    public sealed class FunctionFileService : IFunctionFileService
    {
        private readonly HttpClient _httpClient;
        private readonly FunctionsOptions _functions;

        public FunctionFileService(IOptions<FunctionsOptions> functions, HttpClient httpClient)
        {
            _functions = functions.Value;
            _httpClient = httpClient;
        }

        public async Task UploadAsync(IFormFile file, string fileName)
        {
            if (file is null || file.Length == 0) return;
            if (string.IsNullOrWhiteSpace(_functions.BaseUrl))
                throw new InvalidOperationException("Functions:BaseUrl is not configured.");

            using var content = new StreamContent(file.OpenReadStream());
            content.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_functions.BaseUrl.TrimEnd('/')}/files/{Uri.EscapeDataString(fileName)}")
            {
                Content = content
            };
            if (!string.IsNullOrWhiteSpace(_functions.FilesFunctionKey))
                request.Headers.Add("x-functions-key", _functions.FilesFunctionKey);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"AzureFilesFunction returned {(int)response.StatusCode} ({response.ReasonPhrase}). {detail}");
            }
        }
    }
}