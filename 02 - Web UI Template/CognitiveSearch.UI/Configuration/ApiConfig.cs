namespace CognitiveSearch.UI.Configuration
{
    public class ApiConfig
    {
        //public string Protocol { get; set; } = "https";
        public string BaseUrl { get; set; }
        public string Url => 
           BaseUrl.EndsWith("/")
            ? $"{BaseUrl}api"
            : $"{BaseUrl}/api";
    }
}