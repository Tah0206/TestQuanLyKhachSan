using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace KhachSanTest.Pages
{
    public class RolePermissionPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public RolePermissionPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // ===== LOCATOR =====
        private By userRows = By.CssSelector("table tbody tr");
        private By btnEdit = By.CssSelector(".btn-edit");

        private By ddlRole = By.Id("RoleId");
        private By btnSave = By.CssSelector("button.btn-save");

        // ===== ACTION =====

        public void GoToUserManagement()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/Users");
            wait.Until(d => d.Url.Contains("/Users"));
        }

        public void SelectUser(string username)
        {
            var rows = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(userRows));

            foreach (var row in rows)
            {
                if (row.Text.ToLower().Contains(username.ToLower()))
                {
                    row.Click();
                    return;
                }
            }
        }

        public void ClickEdit()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(btnEdit));
            btn.Click();
        }

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
        }

        public void ClearRole()
        {
            var select = new SelectElement(wait.Until(ExpectedConditions.ElementExists(ddlRole)));
            select.SelectByIndex(0);
        }

        public void OpenRoleDropdown()
        {
            var ddl = wait.Until(ExpectedConditions.ElementExists(ddlRole));
            ddl.Click();
        }

        public void ClickSave()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(btnSave));
            btn.Click();
        }
    }
}