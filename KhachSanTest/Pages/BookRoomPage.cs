using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace KhachSanTest.Pages
{
    public class BookRoomPage
    {
        private IWebDriver driver;

        public BookRoomPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // ===== BUTTON =====
        public void ClickAddBooking()
        {
            driver.FindElement(By.XPath("//a[contains(@href,'Create')]")).Click();
        }

        public void ClickCreate()
        {
            driver.FindElement(By.XPath("//button[contains(text(),'Tạo Đặt Phòng')]")).Click();
        }

        // ===== DROPDOWN (FIX LỖI Ở ĐÂY) =====

        public void SelectCustomer(string name)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            var element = wait.Until(d => d.FindElement(By.Name("CustomerId")));

            var dropdown = new SelectElement(element);
            dropdown.SelectByText(name.Trim());

            // 🔥 verify lại (tránh bị reset)
            wait.Until(d =>
            {
                var selected = new SelectElement(d.FindElement(By.Name("CustomerId")))
                                .SelectedOption.Text.Trim();
                return selected.Contains(name.Trim());
            });
        }

        public void SelectRoom(string room)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            var element = wait.Until(d => d.FindElement(By.Name("RoomId")));

            var dropdown = new SelectElement(element);
            dropdown.SelectByText(room.Trim());

            wait.Until(d =>
            {
                var selected = new SelectElement(d.FindElement(By.Name("RoomId")))
                                .SelectedOption.Text.Trim();
                return selected.Contains(room.Trim());
            });
        }

        public void SelectPaymentStatus(string status)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            var element = wait.Until(d => d.FindElement(By.Name("PaymentStatus")));

            var dropdown = new SelectElement(element);
            dropdown.SelectByText(status.Trim());
        }

        public void SelectRoomStatus(string status)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            var element = wait.Until(d => d.FindElement(By.Name("RoomStatus")));

            var dropdown = new SelectElement(element);
            dropdown.SelectByText(status.Trim());
        }

        // ===== INPUT =====

        public void EnterCheckIn(string date)
        {
            var el = driver.FindElement(By.Name("CheckInDate"));
            el.Clear();
            el.SendKeys(date);
        }

        public void EnterCheckOut(string date)
        {
            var el = driver.FindElement(By.Name("CheckOutDate"));
            el.Clear();
            el.SendKeys(date);
        }

        public void EnterPeople(string num)
        {
            var el = driver.FindElement(By.Name("NumberOfPeople"));
            el.Clear();
            el.SendKeys(num);
        }
    }
}