using Core.Models;
using Core.Scraper;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using RssFeed;
using System;

namespace SomeTests
{
    public class Tests
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        [Test]
        public void Scrape_AnyState_ReturnsFeed()
        {
            var configOptions = new ConfigOptions
            {
                UserName = "",
                Password = "",
                Url = "https://www.freelancer.com/dashboard",
                FeedUrl = "https://www.freelancer.com/ajax/notify/live-feed/pre-populated.php"
            };

            bool result = true;
            using (NewsFeedScraper scraper = new NewsFeedScraper())
            {
                try
                {
                    scraper.GoToDashboard(configOptions);
                    Feed feed = scraper.TryScrapeFeed(configOptions);

                    if (feed != null && feed.Items != null)
                    {
                        foreach (var item in feed.Items)
                        {
                            _logger.Trace($"{item.Id} - {item.JobString} - {item.Url} - {item.Description}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    scraper.SaveSource();
                    result = false;
                }
            }

            result.Should().BeTrue();
        }
    }
}