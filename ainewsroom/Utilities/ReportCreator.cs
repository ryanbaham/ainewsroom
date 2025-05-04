using Markdig;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0101
#pragma warning disable SKEXP0110
namespace ainewsroom.Utilities
{
    internal static class ReportCreator
    {
        public static string ReportBasePath = Path.Combine(Environment.CurrentDirectory, "ai-reports");
        public static JsonSerializerOptions JsonSerializerOpts = new JsonSerializerOptions { WriteIndented = true };

        public static async Task<(string reportPath, string historyPath)> GenerateReport(string html, AgentGroupChat chat)
        {
           
            var ReportName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var ChatExportName = $"history_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var ReportFilePath = Path.Combine(Environment.CurrentDirectory, "ai-reports", ReportName);
            var HistoryFilePath = Path.Combine(Environment.CurrentDirectory, "ai-reports", ChatExportName);

            if (!Directory.Exists(ReportBasePath))
            {
                try { Directory.CreateDirectory(ReportBasePath); }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating report directory: {ex.Message}");
                    return (string.Empty, string.Empty);
                }
            }

            try
            {
                var history = await chat.GetChatMessagesAsync().Reverse().ToListAsync();
                var chatjson = JsonSerializer.Serialize(history, JsonSerializerOpts);

                File.WriteAllText(Path.Combine(ReportBasePath, ChatExportName), chatjson ?? string.Empty);
                File.WriteAllText(Path.Combine(ReportBasePath, ReportName), html ?? string.Empty);

                return (ReportFilePath,HistoryFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing report: {ex.Message}");
                return (string.Empty, string.Empty);
            }

        }


    }
}
