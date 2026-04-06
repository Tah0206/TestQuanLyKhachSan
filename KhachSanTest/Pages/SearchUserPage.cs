using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace KhachSanTest.Pages
{
    public class SearchUserPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public SearchUserPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        private By txtSearch = By.Id("searchString");
        private By btnSearch = By.CssSelector("button[type='submit']");

        public void GoToUsers()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/Users");
            wait.Until(d => d.Url.Contains("/Users"));
        }

        public void EnterSearch(string value)
        {
            var e = wait.Until(ExpectedConditions.ElementExists(txtSearch));
            e.Clear();
            e.SendKeys(value);
        }

        public void ClickSearch()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnSearch)).Click();
        }
    }
}