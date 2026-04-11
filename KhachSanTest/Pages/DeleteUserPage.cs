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

        // FIX: Delete.cshtml dùng "btn-delete" cho nút Xóa, "btn-cancel" cho nút Hủy
        private By btnConfirm = By.CssSelector("button.btn-delete");
        private By btnCancel = By.CssSelector("a.btn-cancel");

        public void GoToUsers()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/Users");
            wait.Until(d => d.Url.Contains("/Users"));
        }

        // FIX: Click link "btn-delete-user" trong hàng để vào trang Delete (thay vì click td text)
        public void SelectUser(string username)
        {
            var rows = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(
                By.CssSelector("table tbody tr")));

            foreach (var row in rows)
            {
                if (row.Text.ToLower().Contains(username.ToLower()))
                {
                    var deleteLink = row.FindElement(By.CssSelector("a.btn-delete-user"));

                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", deleteLink);
                    wait.Until(ExpectedConditions.ElementToBeClickable(deleteLink));

                    try
                    {
                        deleteLink.Click();
                    }
                    catch
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", deleteLink);
                    }

                    // Chờ trang Delete load xong
                    wait.Until(ExpectedConditions.ElementExists(btnConfirm));
                    return;
                }
            }

            throw new Exception("Không tìm thấy user: " + username);
        }

        // Giữ lại để tương thích với step "xóa" / "nhấn nút xóa"
        // SelectUser() đã navigate vào trang Delete nên không cần làm gì thêm
        public void ClickDelete() { }

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