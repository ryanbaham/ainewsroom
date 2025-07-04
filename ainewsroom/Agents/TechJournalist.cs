﻿using ainewsroom.Utilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace ainewsroom.Agents
{
    public class TechJournalist
    {
        public ChatCompletionAgent Agent { get; set; }
        public TechJournalist(Kernel kernel, Setup setup)
        {
            Agent =
            new()
            {
                Name = "Tech-Journalist-Agent",
                Kernel = kernel.Clone(),
                Arguments = new KernelArguments(
                        new OpenAIPromptExecutionSettings()
                        {
                            ServiceId = "openAI",
                            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                            ResponseFormat = typeof(TechJournalistResultModel)
                        }),
                Description = "Journalist that writes on technology topics.",
                Instructions = setup.GetPrompt(this.GetType().Name),
                
            };
        }
    }
}
