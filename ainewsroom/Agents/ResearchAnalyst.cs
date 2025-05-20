using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ainewsroom.Utilities;
using Microsoft.SemanticKernel.ChatCompletion;
#pragma warning disable SKEXP0070
namespace ainewsroom.Agents
{
    public class ResearchAnalyst
    {
        public ChatCompletionAgent Agent { get; set; }
        public ResearchAnalyst(Kernel kernel, Setup setup)
        {
            Agent =
            new()
            {
                Name = "Research-Analyst-Agent",
                Kernel = kernel.Clone(),
                Arguments = new KernelArguments(
                        new OpenAIPromptExecutionSettings()
                        { 
                            ServiceId = "openAI_thinking",
                            FunctionChoiceBehavior =  FunctionChoiceBehavior.Auto(),
                            ResponseFormat = typeof(ResearchAnalystResultModel),
                            ReasoningEffort = "high"

                        }),
                Description = "Agent that searches the web for source material and other journalism and then organizes into themes.", 
                Instructions = setup.GetPrompt(this.GetType().Name),
                InstructionsRole = AuthorRole.Developer
            };
            
    }
        
    }
}
