namespace abc.Models
{
    public class FunctionsOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string TableFunctionKey { get; set; } = string.Empty;
        public string BlobFunctionKey { get; set; } = string.Empty;
        public string QueueFunctionKey { get; set; } = string.Empty;
        public string FilesFunctionKey { get; set; } = string.Empty;
    }
}