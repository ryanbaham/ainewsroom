using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ainewsroom.Utilities;
using ainewsroom.Agents;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using System.Text.Json;
using Microsoft.Extensions.Logging.Console;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Json.Serialization;


#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001
namespace ainewsroom
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            Setup Setup = new Setup();

            Console.WriteLine("Warming Up.........");

            Console.WriteLine("Configuring Plugins.........");
            ITextSearch tavilySearch = new TavilyTextSearch(Setup.Tavily.ApiKey,Setup.Tavily.Options);
            var tavilyPlugin = tavilySearch.CreateWithGetTextSearchResults("TavilySearch");

            Console.WriteLine("Bootstrapping Kernel Instance.........");
            
            // Create a kernel and attach core services
            var builder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(Setup.OpenAI.ChatModel, Setup.OpenAI.ApiKey, serviceId: "openAI")
                .AddOpenAIChatCompletion("o4-mini", Setup.OpenAI.ApiKey, serviceId: "openAI_thinking");
              //.AddGoogleAIGeminiChatCompletion(Setup.GEMINImodelId, Setup.GEMINIapiKey, serviceId: "gemini");
            
            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole(opts =>
                {
                    opts.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                    opts.UseUtcTimestamp = true;
                    opts.IncludeScopes = true;

                });

                logging.AddFilter<ConsoleLoggerProvider>(
                    category: null,
                    level: LogLevel.Information
                );

                var fileOpts = new FileLoggerOptions
                {
                    LogDirectory = "logs",
                    FileName = "trace.log",
                    MinLevel = LogLevel.Trace
                };
                logging.AddProvider(new FileLoggerProvider(fileOpts));
                logging.SetMinimumLevel(LogLevel.Trace);

                
            });
            builder.Services.AddSingleton(sp => new HtmlRenderer(sp, sp.GetRequiredService<ILoggerFactory>()));
            builder.Services.AddScoped<BlazorRenderer>();
            builder.Plugins.Add(tavilyPlugin);
            builder.Plugins.AddFromType<TimePlugin>();


            Console.WriteLine("Sealing Kernel Instance.........");
            Kernel kernel = builder.Build();

            Console.WriteLine("Onboarding Agents.........");
            ResearchAnalyst researchAnalyst = new ResearchAnalyst(kernel, Setup);
            EditorialWriter editorialWriter = new EditorialWriter(kernel, Setup);
            TechJournalist techJournalist = new TechJournalist(kernel, Setup);

            var ResearchAgent = researchAnalyst.Agent;
            var JournalistAgent = techJournalist.Agent;
            var EditorialAgent = editorialWriter.Agent;

            Console.WriteLine("Enabling Agent Collaboration.........");
            KernelFunction selectionFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                 $$$"""
                    You are the coordinator for a three-agent newsroom. 

                    Examine the provided RESPONSE and choose the next participant.
                    State only the name of the chosen participant without explanation.
                    Never choose the participant named in the RESPONSE.

                    ─ Participants ─
                    • {{{ResearchAgent.Name}}}        – posts JSON with a top-level key `"themes"`.
                    • {{{JournalistAgent.Name}}} – posts JSON with a top-level key `"columns"`.
                    • {{{EditorialAgent.Name}}}      – posts JSON with a top-level key `"headline"`.

                    ─ Routing Rules ─
                    1. **If the RESPONSE is a question**, route it to the agent who can answer
                        (e.g., a request for “more sources” → {{{ResearchAgent.Name}}}).
                    2. **If RESPONSE came from {{{ResearchAgent.Name}}}**  
                        and contains `"themes"`, choose {{{JournalistAgent.Name}}}.
                    3. **If RESPONSE came from {{{JournalistAgent.Name}}}**  
                        and contains `"columns"`, choose {{{EditorialAgent.Name}}}.
                    4. **If RESPONSE came from {{{EditorialAgent.Name}}}**  
                        and contains `"headline"`, choose {{{ResearchAgent.Name}}} to start a new cycle.
                    5. If none of the above rules match, default to {{{ResearchAgent.Name}}}.

                    Output **only** the name of the next participant—no other text.
                    Never select the same participant who authored the RESPONSE.

                    RESPONSE:
                    {{$lastmessage}}
                    """,

                
            safeParameterNames: "lastmessage");

            const string TerminationToken = "[all-work-complete]";

            KernelFunction terminationFunction =
                AgentGroupChat.CreatePromptFunctionForStrategy(
                    $$$"""
                        Examine the RESPONSE and determine whether the content has been deemed satisfactory.
                        If content is satisfactory, respond with a single word without explanation: {{{TerminationToken}}}.
                        If specific suggestions are being provided, it is not satisfactory.
                        If no correction is suggested, it is satisfactory.
                        When the content is NOT satisfactory, include a brief explanation of why it is not satisfactory.

                        RESPONSE:
                        {{$lastmessage}}
                        """,
                    safeParameterNames: "lastmessage");



            ChatHistorySummarizationReducer historyReducer = new(kernel.GetRequiredService<IChatCompletionService>("openAI"), 20);

            AgentGroupChat chat =
                new(ResearchAgent, JournalistAgent, EditorialAgent)
                {
                    ExecutionSettings = new AgentGroupChatSettings
                    {
                        SelectionStrategy =
                            new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                            { 
                                // Start with this agent.
                                InitialAgent = ResearchAgent,
                                // Save tokens by only including the final response
                                HistoryReducer = historyReducer,
                                // The prompt variable name for the history argument.
                                HistoryVariableName = "lastmessage",
                                // Returns the entire result value as a string.
                                //ResultParser = (result) => result.GetValue<string>() ?? ResearchAgent.Name
                            },
                        TerminationStrategy =
                            new KernelFunctionTerminationStrategy(terminationFunction, kernel)
                            {
                                // This agent has authority to end the activity
                                Agents = [EditorialAgent],
                                // Save tokens by only including the final response
                                HistoryReducer = historyReducer,
                                // The prompt variable name for the history argument.
                                HistoryVariableName = "lastmessage",
                                // Limit total number of turns
                                MaximumIterations = 20,
                                // Customer result parser to determine if the response is "yes"
                                ResultParser = (result) => result.GetValue<string>()?.Contains(TerminationToken, StringComparison.OrdinalIgnoreCase) ?? false
                            }
                    }
                };

            Console.WriteLine("Newsroom Ready!");
            Console.WriteLine("-------------------------");
            Console.WriteLine("Available Agents:");
            
            foreach (Agent agent in chat.Agents)
            {
                Console.WriteLine($"{agent.Name} - {agent.Description}");
            }
            Console.WriteLine("-------------------------");


            var reportobjectlist = new List<IAgentResult>();
            
            bool isComplete = false;
            do
            {
                Console.WriteLine();
                Console.Write("User > ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }
                input = input.Trim();
                if (input.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                {
                    isComplete = true;
                    break;
                }

                if (input.Equals("RESET", StringComparison.OrdinalIgnoreCase))
                {
                    await chat.ResetAsync();
                    Console.WriteLine("[Conversation has been reset]");
                    continue;
                }  

                if (input.Equals("CHATHISTORY", StringComparison.OrdinalIgnoreCase))
                {
                    var path = ChatHistoryExporter.ToPrettyHtml(await chat.GetChatMessagesAsync().ToListAsync<ChatMessageContent>());
                    Console.WriteLine($"[REPORT has been Generated] - {path} ");
                    continue;
                }

                if (input.Equals("CREATEREPORT", StringComparison.OrdinalIgnoreCase))
                {
                    // Create Templates directory if it doesn't exist
                    var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
                    Directory.CreateDirectory(templatesDir);

                    // Ensure newspaper template exists
                    var templatePath = Path.Combine(templatesDir, "Newspaper.cshtml");

                    // Create the service
                    var templateService = new RazorTemplateService();

                    // Get the most recent result of each type
                    var researchResult = reportobjectlist.OfType<ResearchAnalystResultModel>().LastOrDefault() ?? new ResearchAnalystResultModel();
                    var techResult = reportobjectlist.OfType<TechJournalistResultModel>().LastOrDefault() ?? new TechJournalistResultModel();
                    var editorialResult = reportobjectlist.OfType<EditorialWriterResultModel>().LastOrDefault() ?? new EditorialWriterResultModel();

                    // Render the template
                    string html = await templateService.RenderNewspaperAsync(researchResult, techResult, editorialResult);

                    // Generate the report
                    var path = await ReportCreator.GenerateReport(html,chat);
                    Console.WriteLine($"[REPORT has been generated] - {path.reportPath} ");
                    Console.WriteLine($"[History has been exported] - {path.historyPath} ");
                    continue;
                }

                chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

                chat.IsComplete = false;

                try
                {
                    await foreach (ChatMessageContent response in chat.InvokeAsync())
                    {
                        Console.WriteLine();
                        Console.WriteLine($"{response.AuthorName.ToUpperInvariant()}:{Environment.NewLine}{response.Content}");
                        var author = response.AuthorName;

                        IAgentResult reportobject = author switch
                        {

                            "ResearchAnalystAgent" => JsonSerializer.Deserialize<ResearchAnalystResultModel>(response.Content)!,
                            "EditorialWriterAgent" => JsonSerializer.Deserialize<EditorialWriterResultModel>(response.Content)!,
                            "TechJournalistAgent" => JsonSerializer.Deserialize<TechJournalistResultModel>(response.Content)!,
                            _ => throw new NotImplementedException()
                        };
                            
                        reportobjectlist.Add(reportobject);
                        
                    }
                }
                catch (HttpOperationException exception)
                {
                    Console.WriteLine(exception.Message);
                    if (exception.InnerException != null)
                    {
                        Console.WriteLine(exception.InnerException.Message);
                        if (exception.InnerException.Data.Count > 0)
                        {
                            Console.WriteLine(JsonSerializer.Serialize(exception.InnerException.Data, new JsonSerializerOptions() { WriteIndented = true }));
                        }
                    }
                }
            } while (!isComplete);

        }
    }
}
