using Markdig;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ainewsroom.Utilities
{
    internal static class ReportCreator
    {
        public static string ReportBasePath = Path.Combine(Environment.CurrentDirectory, "ai-reports");
        public static string ReportMDName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.md";
        //public string ReportHTMLName { get; set; } = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        //public string ChatHistoryHTMLName { get; set; } = $"ChatHistory_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        public static string ReportMDFilePath = Path.Combine(Environment.CurrentDirectory, "ai-reports", $"report_{DateTime.Now:yyyyMMdd_HHmmss}.md");
        //public string ReportHTMLFilePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "ai-reports", $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        //public string ChatHistoryHTMLFilePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "ai-reports", $"ChatHistory_{DateTime.Now:yyyyMMdd_HHmmss}.html");

        public static string GenerateReport(ChatMessageContent initialresult)
        {
            var pipeline = new MarkdownPipelineBuilder()
                              .UseAdvancedExtensions()     
                              .Build();
            var outDir = Path.Combine(Environment.CurrentDirectory, "ai-reports");

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            try
            {
                File.WriteAllText(Path.Combine(ReportBasePath, ReportMDName), initialresult.Content ?? string.Empty);
                return ReportMDFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing report: {ex.Message}");
                return string.Empty;
            }
            
        }


    }
}
