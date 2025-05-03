using System;
using System.IO;
using System.Threading.Tasks;
using RazorLight;
using ainewsroom.Agents;

namespace ainewsroom.Utilities
{
    public class RazorTemplateService
    {
        private readonly RazorLightEngine _engine;

        public RazorTemplateService()
        {
            // Create a RazorLight engine that loads templates from the embedded resources
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<string> RenderNewspaperAsync(
            ResearchAnalystResultModel researchResult,
            TechJournalistResultModel techResult,
            EditorialWriterResultModel editorialResult)
        {
            var model = new NewspaperModel
            {
                ResearchResult = researchResult,
                TechResult = techResult,
                EditorialResult = editorialResult,
                CurrentDate = DateTime.Now
            };

            // If using an embedded string template
            string templateContent = await File.ReadAllTextAsync("Templates/Newspaper.cshtml");
            return await _engine.CompileRenderStringAsync("newspaper-template", templateContent, model);
        }
    }

    public class NewspaperModel
    {
        public ResearchAnalystResultModel ResearchResult { get; set; }
        public TechJournalistResultModel TechResult { get; set; }
        public EditorialWriterResultModel EditorialResult { get; set; }
        public DateTime CurrentDate { get; set; }
    }
}
