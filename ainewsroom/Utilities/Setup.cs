using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ainewsroom.Utilities
{
    public static class Setup
    {
        public const string OAImodelId = "gpt-4.1";
        public const string OAIapiKey = "";
        public const string GEMINImodelId = "gemini-2.5-pro-preview-03-25";
        public const string GEMINIapiKey = "";

        // Tavily plugin setup
        #pragma warning disable SKEXP0050
        public const string tavilyApiKey = "";

        public static TavilyTextSearchOptions tavilyOptions = new TavilyTextSearchOptions
        {
            Endpoint = new Uri("https://api.tavily.com/search"),
            SearchDepth = TavilySearchDepth.Advanced,
            IncludeAnswer = true,
            //IncludeRawContent = true,
        };

    }
}
