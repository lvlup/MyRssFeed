using RssFeed;
using System;

namespace Core.Scraper
{
    class ScraperException : Exception
    {
        public ScraperException(string message, ConfigOptions config) : base(message)
        {
            Config = config;
        }

        public ConfigOptions Config { get; }

        public override string ToString()
        {
            return $"{Message}. Config: {Config}";
        }
    }
}
