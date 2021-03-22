using System;
using System.IO;
using System.Reflection;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Core.Scraper
{
   public abstract class ScraperBase : IDisposable
    {
        protected readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private bool _disposed;
        public bool Disposed { get { return _disposed; } }
        protected string ProxyAddress;
        public IWebDriver WebDriver { get; private set; }

        protected ScraperBase()
        {
            WebDriver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        public void GoToUrl(string url)
        {
            Logger.Trace("Navigating to url: {0}", url);
            WebDriver.Navigate().GoToUrl(url);
        }

        public IWebElement FindElementWaitSafe(By by, int waitSec = 10)
        {
            try
            {
                return FindElementWait(by, waitSec);
            }
            catch { return null; }
        }

        public IWebElement FindElementWait(By by, int waitSec = 10)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(waitSec));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            IWebElement element = wait.Until<IWebElement>((d) => d.FindElement(@by));
            return element;
        }

        public void SaveSource()
        {
            try
            {
                var fullFilePath = GetPageSourceFullPath();
                File.WriteAllText(fullFilePath, WebDriver.PageSource);
                Logger.Info("Saved page source. File: {0}", fullFilePath);
            }
            catch (Exception exc)
            {
                Logger.Error("Failed to take screenshot. Error: {0}. Stack: {1}", exc.Message, exc.StackTrace);
            }
        }

        private string GetPageSourceFullPath()
        {
            string fileName = Guid.NewGuid() + ".txt";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "page_sources");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, fileName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    WebDriver.Dispose();
                }

                // shared cleanup logic
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    
    }
}