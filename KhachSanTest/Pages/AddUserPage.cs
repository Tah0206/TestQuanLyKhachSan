using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;
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
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Locators
        private By txtUsername = By.Id("Username");
        private By txtPassword = By.Id("PasswordHash");
        private By txtFullName = By.Id("FullName");
        private By txtEmail = By.Id("Email");
        private By ddlRole = By.Id("RoleId");
        private By btnCreate = By.CssSelector("button.btn-submit, button[type='submit']");

        public void ClickAddUser()
        {
            // Tìm nút thêm và click
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.btn-add-user")));
            btn.Click();
            // Chờ form load xong
            wait.Until(ExpectedConditions.ElementIsVisible(txtUsername));
        }

        private void EnterInput(By locator, string value)
        {
            var element = wait.Until(ExpectedConditions.ElementIsVisible(locator));
            element.Click();
            element.Clear();

            // Nếu data từ Excel bị null, gán tạm chuỗi rỗng để không crash code
            string textToType = string.IsNullOrEmpty(value) ? "" : value;

            element.SendKeys(textToType);
            element.SendKeys(Keys.Tab); // Nhảy sang ô tiếp theo
            Thread.Sleep(300); // Nghỉ 0.3s để máy kịp nhận
        }

        public void EnterUsername(string v) => EnterInput(txtUsername, v);
        public void EnterPasswordHash(string v) => EnterInput(txtPassword, v);
        public void EnterFullName(string v) => EnterInput(txtFullName, v);
        public void EnterEmail(string v) => EnterInput(txtEmail, v);

        public void SelectRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return;
            var select = new SelectElement(wait.Until(ExpectedConditions.ElementIsVisible(ddlRole)));
            try
            {
                select.SelectByText(role);
            }
            catch
            {
                // Nếu không tìm thấy text thì chọn cái đầu tiên hoặc bỏ qua
            }
        }

        public void ClickCreate()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(btnCreate));
            btn.Click();
            Thread.Sleep(2000); // Chờ server xử lý và chuyển trang
        }
    }
}