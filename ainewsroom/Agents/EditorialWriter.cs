using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ainewsroom.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ainewsroom.Agents
{
    public class EditorialWriter
    {
        public ChatCompletionAgent Agent { get; set; }
        public EditorialWriter(Kernel kernel, Setup setup)
        {
            Agent =
            new()
            {
                Name = "Editorial-Writer-Agent",
                Kernel = kernel.Clone(),
                Arguments = new KernelArguments(
                        new OpenAIPromptExecutionSettings()
                        { 
                            ServiceId = "openAI_writing",
                            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                            ResponseFormat = typeof(EditorialWriterResultModel),
                            //ReasoningEffort = "high",

                        }),
                Description = "Writes editorial opinion for the AI newsroom. Utilizes the work of the analysts and journalists in the newsroom.",
                Instructions = setup.GetPrompt(this.GetType().Name),
                
            };
        }
        
    }
}
