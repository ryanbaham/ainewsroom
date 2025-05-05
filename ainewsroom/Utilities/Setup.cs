using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using System.Reflection;
using Microsoft.Extensions.Configuration;

#pragma warning disable SKEXP0050

namespace ainewsroom.Utilities
{

    public class Setup
    {
        private readonly IConfigurationRoot configRoot;

        private TavilySettings tavily;
        private OpenAISettings openAI;
        private Dictionary<string,string> prompts;

        public TavilySettings Tavily => this.tavily ??= this.GetSettings<TavilySettings>();
        public OpenAISettings OpenAI => this.openAI ??= this.GetSettings<OpenAISettings>();
        //public Dictionary<string, string> Prompts => this.prompts ??= this.LoadPrompts();

        public TSettings GetSettings<TSettings>() =>
        this.configRoot.GetRequiredSection(typeof(TSettings).Name).Get<TSettings>()!;

        public Setup()
        {
            this.configRoot =
                new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                    .Build();
            LoadPrompts();
           
        }

        public string GetPrompt(string promptName)
        {
            if (this.prompts == null)
            {
                this.prompts = LoadPrompts();
            }
            try { return this.prompts[promptName]; }
            catch { Console.WriteLine($"Prompt {promptName} not found."); return string.Empty; }
        }

        private Dictionary<string, string> LoadPrompts()
        {
            var prompts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string promptsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts");

            if (!Directory.Exists(promptsDirectory))
            {
                Directory.CreateDirectory(promptsDirectory);
                Console.WriteLine($"[Setup: Prompts Directory] = {promptsDirectory}");
                return prompts;
            }

            string[] promptFiles = Directory.GetFiles(promptsDirectory, "*.prompt");

            foreach (string promptFile in promptFiles)
            {
                try
                {
                    string content = File.ReadAllText(promptFile);
                    string promptName = Path.GetFileNameWithoutExtension(promptFile);
                    prompts[promptName] = content;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading prompt file {promptFile}: {ex.Message}");
                }
            }

            return prompts;
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
