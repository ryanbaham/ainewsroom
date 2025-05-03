using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ainewsroom.Utilities
{

    public class Setup
    {
        private readonly IConfigurationRoot configRoot;

        private TavilySettings tavily;
        private OpenAISettings openAI;

        public TavilySettings Tavily => this.tavily ??= this.GetSettings<TavilySettings>();
        public OpenAISettings OpenAI => this.openAI ??= this.GetSettings<OpenAISettings>();


#pragma warning disable SKEXP0050

        public TavilyTextSearchOptions tavilyOptions => new TavilyTextSearchOptions
        {
            Endpoint = new Uri(Tavily.Uri),
            SearchDepth = TavilySearchDepth.Advanced,
            IncludeAnswer = true,
            //IncludeRawContent = true,
        };

        public TSettings GetSettings<TSettings>() =>
        this.configRoot.GetRequiredSection(typeof(TSettings).Name).Get<TSettings>()!;

        public Setup()
        {
            this.configRoot =
                new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                    .Build();
        }
    }
    public class TavilySettings
    {
        public string Uri { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public TavilyTextSearchOptions Options => new TavilyTextSearchOptions
        {
            Endpoint = new Uri(Uri),
            SearchDepth = TavilySearchDepth.Advanced,
            IncludeAnswer = true,
            //IncludeRawContent = true,
        };
    }
    public class OpenAISettings
    {
        public string ChatModel { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
