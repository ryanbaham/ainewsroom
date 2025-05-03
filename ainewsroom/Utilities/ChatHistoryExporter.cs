using System.Text;
using Markdig;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

namespace ainewsroom.Utilities
{
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0101
    public static class ChatHistoryExporter
    {
        /// <summary>
        /// Renders SK ChatHistory to a styled, standalone HTML string.
        /// </summary>
        /// 

        public static string ReportBasePath = Path.Combine(Environment.CurrentDirectory, "ai-reports");
        //public string ReportHTMLName { get; set; } = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        public static string ChatHistoryHTMLName { get; set; } = $"ChatHistory_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        //public static string ReportMDFilePath = Path.Combine(Environment.CurrentDirectory, "ai-reports", $"report_{DateTime.Now:yyyyMMdd_HHmmss}.md");
        //public string ReportHTMLFilePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "ai-reports", $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        public static string ChatHistoryHTMLFilePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "ai-reports", $"ChatHistory_{DateTime.Now:yyyyMMdd_HHmmss}.html");

        public static string ToPrettyHtml(IEnumerable<ChatMessageContent> history,
                                          string? title = null,
                                          string? cssPath = null)
        {
            // 1️⃣ Markdig pipeline (tweak extensions as you like)
            var pipeline = new MarkdownPipelineBuilder()
                             .UseAdvancedExtensions()
                             .Build();

            // 2️⃣ Convert each SK message
            var sb = new StringBuilder();
            var sbmetadata = new StringBuilder();
            foreach (ChatMessageContent msg in history.Reverse())
            {

                string innerHtml = Markdown.ToHtml(msg.Content ?? string.Empty, pipeline);
                sb.Append($$"""
                <div class="msg {{msg.AuthorName ?? msg.Role.Label}}">
                <p>Actor: {{msg.AuthorName ?? msg.Role.Label}}</p>
                {{sbmetadata}}
                <hr>
                  {{innerHtml}}
                </div>
                
                """);
            }

            // 3️⃣ Load CSS (either embedded string or external file)
            string css = cssPath is not null
                         ? File.ReadAllText(cssPath)
                         : EmbeddedCss;    // see §2

            // 4️⃣ Build the full document
            var html = $$"""
                        <!DOCTYPE html>
                        <html lang="en">
                        <head>
                          <meta charset="utf-8" />
                          <meta name="viewport" content="width=device-width,initial-scale=1" />
                          <title>{{title ?? "Chat Transcript"}}</title>
                          <style>{{css}}</style>
                        </head>
                        <body>
                          <main class="chat">
                            {{sb}}
                          </main>
                        </body> 
                        </html>
                        """;

            try
            {
                File.WriteAllText(Path.Combine(ReportBasePath, ChatHistoryHTMLName), html ?? string.Empty);
                return ChatHistoryHTMLFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing report: {ex.Message}");
                return string.Empty;
            }
        }

        // Optional: embed the stylesheet so callers don’t need an extra file
        private const string EmbeddedCss = """
    
    :root{--bg:#fff;--fg:#24292e;--bubble-user:#d6eaff;--bubble-assistant:#f6f8fa;
          --border:#d0d7de;--code-bg:#f6f8fa;--code-fg:#d73a49;
          --link:#0969da;--link-hover:#054289;font-family:-apple-system,
          BlinkMacSystemFont,"Segoe UI",Roboto,Oxygen,Ubuntu,Cantarell,"Open Sans",
          "Helvetica Neue",sans-serif;font-size:16px;line-height:1.6;color-scheme:light dark;}
    @media (prefers-color-scheme: dark){
      :root{--bg:#0d1117;--fg:#c9d1d9;--bubble-user:#00325e;--bubble-assistant:#161b22;
            --border:#30363d;--code-bg:#161b22;--link:#58a6ff;--link-hover:#79c0ff;}
    }
    body{margin:0;background:var(--bg);color:var(--fg);}
    .chat{display:flex;flex-direction:column;gap:1rem;max-width:760px;
          margin:0 auto;padding:2rem 1rem;}
    .msg{max-width:80%;padding:.75rem 1rem;border-radius:12px;
         border:1px solid var(--border);word-wrap:break-word;}
    .msg.user{background:var(--bubble-user);margin-left:auto;}
    .msg.assistant{background:var(--bubble-assistant);margin-right:auto;}
    .msg.assistant{background:var(--bubble-assistant);margin-right:auto;}
    .msg.system,.msg.tool{background:var(--bubble-assistant);margin-right:auto;
                          font-size:.9rem;color:#6a737d;text-align:left;}
    /* markdown inside bubbles */
    .msg h1,.msg h2{margin-top:0;font-size:1.25rem;border-bottom:1px solid var(--border);
                    padding-bottom:.25em;}
    .msg h3{margin-top:0;font-size:1.1rem;}
    .msg p{margin:.75em 0;}
    .msg a{color:var(--link);text-decoration:none;}
    .msg a:hover{color:var(--link-hover);text-decoration:underline;}
    .msg code{background:var(--code-bg);color:var(--code-fg);padding:.15em .35em;
              border-radius:4px;font-family:ui-monospace,SFMono-Regular,Menlo,Consolas,
              "Liberation Mono",monospace;}
    .msg pre{background:var(--code-bg);padding:1em;border-radius:8px;overflow:auto;
             margin:1em 0;}
    .msg pre code{background:none;padding:0;color:inherit;}
    .msg ul,.msg ol{margin:.5em 0 .5em 1.2em;}
    .msg blockquote{margin:.8em 0;padding-left:1em;color:#6a737d;
                    border-left:.25em solid var(--border);}
    .msg table{border-collapse:collapse;width:100%;margin:1em 0;}
    .msg th,.msg td{border:1px solid var(--border);padding:.4em .8em;}
    .msg thead{background:var(--code-bg);font-weight:600;}
    .msg img{max-width:100%;}
    """;
    }

}