using Markdig;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0101
namespace ainewsroom.Utilities
{
    internal static class ReportCreator
    {
        public static string ReportBasePath = Path.Combine(Environment.CurrentDirectory, "ai-reports");
        

        public static string GenerateReport(string html)
        {
           
            var ReportName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var ReportFilePath = Path.Combine(Environment.CurrentDirectory, "ai-reports", ReportName);

            try
            {
                File.WriteAllText(Path.Combine(ReportBasePath, ReportName), html ?? string.Empty);
                return ReportFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing report: {ex.Message}");
                return string.Empty;
            }

        }


    }
}
