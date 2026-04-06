using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace KhachSanTest.Pages
{
    public class DeleteUserPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public DeleteUserPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        private By btnDelete = By.CssSelector(".btn-delete");
        private By btnConfirm = By.CssSelector(".btn-danger");
        private By btnCancel = By.CssSelector(".btn-secondary");

        public void GoToUsers()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/Users");
            wait.Until(d => d.Url.Contains("/Users"));
        }

        public void SelectUser(string username)
        {
            var user = wait.Until(d => d.FindElement(By.XPath($"//td[contains(text(),'{username}')]")));
            user.Click();
        }

        public void ClickDelete()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnDelete)).Click();
        }

        public void ConfirmDelete()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnConfirm)).Click();
        }

        public void CancelDelete()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnCancel)).Click();
        }
    }
}