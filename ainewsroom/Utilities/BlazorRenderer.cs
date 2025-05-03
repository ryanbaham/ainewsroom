using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ainewsroom.Utilities
{
    public class BlazorRenderer
    {
        private readonly HtmlRenderer _htmlRenderer;
        public BlazorRenderer(HtmlRenderer htmlRenderer) => _htmlRenderer = htmlRenderer;

        public Task<string> RenderComponentAsync<T>(Dictionary<string, object?> parameters)
            where T : IComponent
        {
            var pv = ParameterView.FromDictionary(parameters);
            return _htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var htmlRoot = await _htmlRenderer.RenderComponentAsync<T>(pv);
                return htmlRoot.ToHtmlString();
            });
        }
    }
}
