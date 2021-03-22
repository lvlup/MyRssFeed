using Core.Models;
using Newtonsoft.Json;
using OpenQA.Selenium;
using RssFeed;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Scraper
{
  public  class NewsFeedScraper : ScraperBase
    {
        public void GoToDashboard(ConfigOptions configOptions)
        {
            int maxRetries = 5;
            int retry = 0;
            while (retry < maxRetries)
            {
                GoToUrl(configOptions.Url);
                if (Regex.IsMatch(WebDriver.Url, @"/login") || WebDriver.Url == configOptions.Url)
                {
                    break;
                }

                retry++;
            }

            if (WebDriver.Url != configOptions.Url)
            {
                Login(configOptions);
            }
        }

        public void Login(ConfigOptions configOptions)
        {
            var userNameInput = FindElementWaitSafe(By.XPath("/html/body/app-root/app-logged-out-shell/app-login-page/fl-container/fl-bit/app-login/app-credentials-form/form/fl-input[1]/fl-bit/fl-bit/input"), 1);
            var passwordInput = FindElementWaitSafe(By.XPath("/html/body/app-root/app-logged-out-shell/app-login-page/fl-container/fl-bit/app-login/app-credentials-form/form/fl-input[2]/fl-bit/fl-bit/input"), 1);
            var loginBtn = FindElementWaitSafe(By.XPath("/html/body/app-root/app-logged-out-shell/app-login-page/fl-container/fl-bit/app-login/app-credentials-form/form/app-login-signup-button/fl-button/button"), 1);

            if (userNameInput == null || passwordInput == null || loginBtn == null)
                throw new ScraperException("Invalid login page", configOptions);

            userNameInput.Clear();
            passwordInput.Clear();

            userNameInput.SendKeys(configOptions.UserName);
            passwordInput.SendKeys(configOptions.Password);

            loginBtn.Click();
        }

        public Feed TryScrapeFeed(ConfigOptions configOptions)
        {
            CookieContainer cookieJar = new CookieContainer();
            var cookies = WebDriver.Manage().Cookies.AllCookies;
            foreach (var cookie in cookies)
            {
                cookieJar.Add(new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
            }

            int maxRetries = 5;
            int retry = 0;

            Logger.Trace("Downloading {0}", configOptions.FeedUrl);

            Feed feed = null;

            while (true)
            {
                if (retry >= maxRetries)
                    throw new ScraperException("Can't download project feed. Retries exceeded", configOptions);

                try
                {
                    using (var client = new CookieWebClient(cookieJar))
                    {
                        if (!string.IsNullOrEmpty(ProxyAddress))
                        {
                            client.Proxy = new WebProxy(ProxyAddress);
                        }

                        var mozilaAgent =
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36";
                        client.Headers.Add("User-Agent", mozilaAgent);

                        // Call authenticated resources
                        string result = client.DownloadString(configOptions.FeedUrl);

                        feed = JsonConvert.DeserializeObject<Feed>(result);
                    }

                    break;

                }
                catch (Exception e)
                {
                    Logger.Trace("Failed to download {0} page. Retry {1} of {2}", configOptions.FeedUrl, retry, maxRetries);
                    Logger.Error(e);
                    retry++;
                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                }
            }

            Logger.Info("Successfully downloaded {0}", configOptions.FeedUrl);
            WebDriver.Quit();

            return feed;
        }

    }
}
