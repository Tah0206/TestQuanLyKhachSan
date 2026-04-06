using OpenQA.Selenium;
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
        private By btnDelete = By.XPath("//a[contains(@class,'btn-danger') and contains(text(),'Xóa')]");
        private By btnConfirmDelete = By.XPath("//button[contains(@class,'btn-danger') and contains(text(),'Xóa')]");
        private By statusCancelled = By.XPath("//td[contains(text(),'Đã hủy')]");
        private By errorMessage = By.CssSelector(".alert-danger, .text-danger");

        // ===== ACTION =====

        public void GoToPage()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/RoomBookings");
        }

        public void ClickDelete()
        {
            var elements = driver.FindElements(btnDelete);
            if (elements.Any())
            {
                elements.First().Click();
                Thread.Sleep(1000);
            }
        }

        public void ConfirmDelete()
        {
            driver.FindElement(btnConfirmDelete).Click();
            Thread.Sleep(2000);
        }

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