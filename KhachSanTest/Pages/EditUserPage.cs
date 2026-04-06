using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace KhachSanTest.Pages
{
    public class EditUserPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public EditUserPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // ===== LOCATOR =====

        private By tableRows = By.CssSelector("table tbody tr");

        private By txtUsername = By.Id("Username");
        private By txtPassword = By.Id("PasswordHash");
        private By txtFullName = By.Id("FullName");
        private By txtEmail = By.Id("Email");
        private By ddlRole = By.Id("RoleId");

        private By btnSave = By.CssSelector("button.btn-submit");
        private By btnBack = By.CssSelector("a.btn-secondary");

        // ===== NAVIGATION =====

        public void GoToUsers()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/Users/Create");
            wait.Until(d => d.Url.Contains("/Users"));
        }

        // ===== CLICK EDIT THEO USER (QUAN TRỌNG NHẤT) =====

        public void ClickEditByUsername(string username)
        {
            var rows = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(tableRows));

            foreach (var row in rows)
            {
                if (row.Text.ToLower().Contains(username.ToLower()))
                {
                    var editBtn = row.FindElement(By.XPath(".//a[contains(text(),'Sửa')]"));

                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", editBtn);

                    wait.Until(ExpectedConditions.ElementToBeClickable(editBtn));

                    try
                    {
                        editBtn.Click();
                    }
                    catch
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", editBtn);
                    }

                    // 🔥 CHỜ FORM LOAD
                    wait.Until(ExpectedConditions.ElementExists(txtUsername));
                    return;
                }
            }

            throw new Exception("Không tìm thấy user: " + username);
        }

        // ===== INPUT HELPER (ANTI-FLAKY) =====

        private void EnterInput(By locator, string value)
        {
            var e = wait.Until(ExpectedConditions.ElementIsVisible(locator));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", e);

            try
            {
                e.Click();
                e.Clear();
                e.SendKeys(value);
            }
            catch
            {
                // fallback JS (quan trọng)
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value='';", e);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value=arguments[1];", e, value);
            }
        }

        public void EnterUsername(string value)
        {
            EnterInput(txtUsername, value);
        }

        public void EnterPassword(string value)
        {
            // ⚠ password có thể readonly → dùng JS luôn
            var e = wait.Until(ExpectedConditions.ElementExists(txtPassword));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value='';", e);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value=arguments[1];", e, value);
        }

        public void EnterFullName(string value)
        {
            EnterInput(txtFullName, value);
        }

        public void EnterEmail(string value)
        {
            EnterInput(txtEmail, value);
        }

        // ===== SELECT ROLE (ANTI LỖI DẤU) =====

        public void SelectRole(string role)
        {
            var select = new SelectElement(wait.Until(ExpectedConditions.ElementExists(ddlRole)));

            foreach (var option in select.Options)
            {
                if (option.Text.Trim().ToLower() == role.Trim().ToLower())
                {
                    option.Click();
                    return;
                }
            }

            throw new Exception("Không tìm thấy role: " + role);
        }

        public void ClearRole()
        {
            var select = new SelectElement(wait.Until(ExpectedConditions.ElementExists(ddlRole)));
            select.SelectByIndex(0);
        }

        // ===== ACTION =====

        public void ClickSave()
        {
            var btn = wait.Until(ExpectedConditions.ElementExists(btnSave));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", btn);

            try
            {
                btn.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            }
        }

        public void ClickBack()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(btnBack));
            btn.Click();
        }
    }
}