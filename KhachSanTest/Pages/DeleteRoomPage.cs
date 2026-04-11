using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace KhachSanTest.Pages
{
    public class DeleteRoomPage
    {
        private IWebDriver driver;

        public DeleteRoomPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // ===== ELEMENT =====

        // Nút "Xóa" trên trang Index (class="btn-delete-booking")
        // Trong Index.cshtml: <a href="..." class="btn-delete-booking"><i ...></i> Xóa</a>
        private By btnDeleteOnIndex = By.CssSelector("a.btn-delete-booking");

        // Nút "Xóa" xác nhận trên trang Delete.cshtml (button type=submit, class="btn-delete")
        // Trong Delete.cshtml: <button type="submit" class="btn-delete"><i ...></i> Xóa</button>
        private By btnConfirmDelete = By.CssSelector("button.btn-delete[type='submit']");

        // Trạng thái "Đã hủy" nằm trong <span class="status-badge status-cancelled"> bên trong <td>
        private By statusCancelled = By.CssSelector("span.status-cancelled");

        // Thông báo lỗi
        private By errorMessage = By.CssSelector(".alert-danger, .text-danger");

        // ===== ACTION =====

        public void GoToPage()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/Bookings");
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Click nút Xóa đầu tiên trên danh sách (trang Index)
        /// </summary>
        public void ClickDelete()
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElements(btnDeleteOnIndex).Count > 0);

                var elements = driver.FindElements(btnDeleteOnIndex);
                if (elements.Any())
                {
                    elements.First().Click();
                    Thread.Sleep(1000);
                }
                else
                {
                    throw new NoSuchElementException("Không tìm thấy nút Xóa trên danh sách.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                throw new NoSuchElementException("Timeout: Không tìm thấy nút Xóa (btn-delete-booking) trên trang.");
            }
        }

        /// <summary>
        /// Xác nhận xóa trên trang Delete.cshtml (click nút submit "btn-delete")
        /// </summary>
        public void ConfirmDelete()
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var btn = wait.Until(d => d.FindElement(btnConfirmDelete));
                btn.Click();
                Thread.Sleep(2000);
            }
            catch (WebDriverTimeoutException)
            {
                throw new NoSuchElementException("Timeout: Không tìm thấy nút xác nhận Xóa (button.btn-delete) trên trang Delete.");
            }
        }

        /// <summary>
        /// Kiểm tra xem có record nào có trạng thái "Đã hủy" không
        /// </summary>
        public bool IsDeletedSuccess()
        {
            return driver.FindElements(statusCancelled).Any();
        }

        public string GetError()
        {
            var errors = driver.FindElements(errorMessage);
            return errors.FirstOrDefault()?.Text ?? "";
        }
    }
}