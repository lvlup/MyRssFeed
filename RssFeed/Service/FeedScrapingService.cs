using Core.Extensions;
using Core.Models;
using Core.Scraper;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RssFeed.Service
{
    public interface IFeedScrapingService
    {
        Task GetRssFeed();

        Task<string> GetStringRssFeed();
    }


    class FeedScrapingService : IFeedScrapingService
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IOptions<ConfigOptions> _options;

        public FeedScrapingService(IOptions<ConfigOptions> options)
        {
            _options = options;
        }

        public async Task<string> GetStringRssFeed()
        {
            string[] categories = _options.Value.JobCategories.Split(';');
            Rss rss;
            using (var client = new WebClient())
            {
                string data = await client.DownloadStringTaskAsync("https://www.freelancer.com/rss.xml");
                rss = data.DeserializeXml<Rss>();
                if (rss != null && rss.Feed != null && rss.Feed.Items != null)
                {
                    rss.Feed.Items = rss.Feed.Items.Where(item => item.Categories.Any(c => categories.Any(cc => string.Compare(c, cc, StringComparison.OrdinalIgnoreCase) == 0))).ToArray();
                }
            }

            return await Task.FromResult(rss == null ? "" : rss.SerializeToXml(Encoding.UTF8));
        }

        public Task GetRssFeed()
        {
            _logger.Trace("Starting scraping job");

            using (NewsFeedScraper scraper =  new NewsFeedScraper())
            {
                try
                {
                    scraper.GoToDashboard(_options.Value);
                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                    Feed feed = scraper.TryScrapeFeed(_options.Value);

                    if (feed != null && feed.Items != null)
                    {
                        foreach (var item in feed.Items)
                        {
                            Console.WriteLine($"{item.Id} - {item.JobString} - {item.Url} - {item.Description}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    scraper.SaveSource();
                }
            }
            return Task.FromResult(true);
        }
    }
}
