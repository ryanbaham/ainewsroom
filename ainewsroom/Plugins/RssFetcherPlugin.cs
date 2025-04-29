using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Text.Json;

namespace ainewsroom.Plugins
{
    internal class RssFetcherPlugin
    {
        private readonly HttpClient _http;
        private readonly string?[] _urls;

        public record FeedItem(string Title, string Url, DateTime Published, string Content);
        
        public RssFetcherPlugin(HttpClient httpClient, string?[] urls)
        {
            _http = httpClient;
            _urls = urls;
        }

        [KernelFunction("get_rss_feed_as_JSON")]
        [Description("Fetches the items from an RSS feed URL and returns them as a list, each containing Title, URL, PublishedDate, Content")]
        public async Task<IEnumerable<FeedItem>> GetFeedItemsAsJsonAsync(string feedUrl)
        {
            using var stream = await _http.GetStreamAsync(feedUrl);
            using var reader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(reader);  // parses RSS/Atom :contentReference[oaicite:1]{index=1}

            var items = feed.Items
                            .OrderByDescending(i => i.PublishDate)
                            .Select(i => new FeedItem(
                                i.Title.Text,
                                i.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                                i.PublishDate.DateTime,
                                i.Content.ToString()))
                            .ToList();

            return items;
        }
    }
}
