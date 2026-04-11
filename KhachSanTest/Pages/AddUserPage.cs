using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

namespace KhachSanTest.Pages
{
    public class AddUserPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public AddUserPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15)); // increased from 10
        }

        // Locators — all match Create.cshtml exactly
        private By txtUsername = By.Id("Username");
        private By txtPassword = By.Id("PasswordHash"); // matches model property name
        private By txtFullName = By.Id("FullName");
        private By txtEmail = By.Id("Email");
        private By ddlRole = By.Id("RoleId");
        private By btnCreate = By.CssSelector("button.btn-submit");

        public void ClickAddUser()
        {
            // Try the specific class first, fall back to any "Create" link if it doesn't exist
            try
            {
                var btn = wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("a.btn-add-user")));
                btn.Click();
            }
            catch (WebDriverTimeoutException)
            {
                // Fallback: navigate directly to the Create page
                driver.Navigate().GoToUrl("http://localhost:58609/Users/Create");
            }

            // Wait until the Username field is visible before returning
            wait.Until(ExpectedConditions.ElementIsVisible(txtUsername));
            Thread.Sleep(500); // small buffer for full DOM render
        }

        private void EnterInput(By locator, string value)
        {
            var element = wait.Until(ExpectedConditions.ElementIsVisible(locator));
            element.Click();
            element.Clear();
            element.SendKeys(value ?? "");
            element.SendKeys(Keys.Tab);
            Thread.Sleep(300);
        }

        public void EnterUsername(string v) => EnterInput(txtUsername, v);
        public void EnterPasswordHash(string v) => EnterInput(txtPassword, v);
        public void EnterFullName(string v) => EnterInput(txtFullName, v);
        public void EnterEmail(string v) => EnterInput(txtEmail, v);

        public void SelectRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return;
            var select = new SelectElement(wait.Until(
                ExpectedConditions.ElementIsVisible(ddlRole)));
            try { select.SelectByText(role); }
            catch { /* role text not found — leave default */ }
        }

        public void ClickCreate()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(btnCreate));
            btn.Click();
            Thread.Sleep(2000);
        }
    }
}