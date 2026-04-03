using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace KhachSanTest.Pages
{
    public class LoginPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public LoginPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        }

        // ✅ LOCATOR ĐÚNG CASE THEO HTML
        private By txtUsername = By.Id("Username");
        private By txtPassword = By.Id("Password");
        private By chkRemember = By.Id("RememberMe");
        private By btnLogin = By.CssSelector("button[type='submit']");

        // ===== ACTION =====

        public void EnterUsername(string username)
        {
            var element = wait.Until(ExpectedConditions.ElementIsVisible(txtUsername));
            element.Clear();
            element.SendKeys(username);
        }

        public void EnterPassword(string password)
        {
            var element = wait.Until(ExpectedConditions.ElementIsVisible(txtPassword));
            element.Clear();
            element.SendKeys(password);
        }

        public void ClickRememberMe()
        {
            var checkbox = wait.Until(ExpectedConditions.ElementToBeClickable(chkRemember));
            if (!checkbox.Selected)
                checkbox.Click();
        }

        public void ClickLogin()
        {
            var button = wait.Until(ExpectedConditions.ElementToBeClickable(btnLogin));
            button.Click();
        }
    }
}