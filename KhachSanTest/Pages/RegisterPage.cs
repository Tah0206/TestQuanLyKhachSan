using OpenQA.Selenium;

namespace KhachSanTest.Pages
{
    public class RegisterPage
    {
        private IWebDriver driver;

        public RegisterPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // ===== LOCATOR =====
        private By txtUsername = By.Id("Username");
        private By txtPassword = By.Id("Password");
        private By txtConfirm = By.Id("ConfirmPassword");
        private By txtFullName = By.Id("FullName");
        private By txtEmail = By.Id("Email");

        private By btnRegister = By.CssSelector("button[type='submit']");

        // ===== ACTION =====
        public void EnterUsername(string value)
        {
            var el = driver.FindElement(txtUsername);
            el.Clear();
            el.SendKeys(value);
        }

        public void EnterPassword(string value)
        {
            var el = driver.FindElement(txtPassword);
            el.Clear();
            el.SendKeys(value);
        }

        public void EnterConfirm(string value)
        {
            var el = driver.FindElement(txtConfirm);
            el.Clear();
            el.SendKeys(value);
        }

        public void EnterFullName(string value)
        {
            var el = driver.FindElement(txtFullName);
            el.Clear();
            el.SendKeys(value);
        }

        public void EnterEmail(string value)
        {
            var el = driver.FindElement(txtEmail);
            el.Clear();
            el.SendKeys(value);
        }

        public void ClickRegister()
        {
            driver.FindElement(btnRegister).Click();
        }
    }
}