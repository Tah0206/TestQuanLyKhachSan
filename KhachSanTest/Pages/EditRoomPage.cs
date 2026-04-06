using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace KhachSanTest.Pages
{
    public class EditRoomPage
    {
        private IWebDriver driver;

        public EditRoomPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // ===== ELEMENT =====
        private IWebElement ddlCustomer => driver.FindElement(By.Name("CustomerId"));
        private IWebElement ddlRoom => driver.FindElement(By.Name("RoomId"));
        private IWebElement txtCheckIn => driver.FindElement(By.Name("CheckInDate"));
        private IWebElement txtCheckOut => driver.FindElement(By.Name("CheckOutDate"));
        private IWebElement txtPeople => driver.FindElement(By.Name("NumberOfPeople"));
        private IWebElement txtNote => driver.FindElement(By.Name("Note"));
        private IWebElement ddlPayment => driver.FindElement(By.Name("PaymentStatusID"));
        private IWebElement ddlStatus => driver.FindElement(By.Name("BookingStatusID"));
        private IWebElement btnSave => driver.FindElement(By.CssSelector("button[type='submit']"));
        private IWebElement btnCancel => driver.FindElement(By.LinkText("Back to List"));

        // ===== ACTION =====
        public void SelectCustomer(string name)
        {
            new SelectElement(ddlCustomer).SelectByText(name);
        }

        public void SelectRoom(string room)
        {
            new SelectElement(ddlRoom).SelectByText(room);
        }

        public void EnterCheckIn(string date)
        {
            txtCheckIn.Clear();
            txtCheckIn.SendKeys(date);
        }

        public void EnterCheckOut(string date)
        {
            txtCheckOut.Clear();
            txtCheckOut.SendKeys(date);
        }

        public void EnterPeople(string people)
        {
            txtPeople.Clear();
            txtPeople.SendKeys(people);
        }

        public void EnterNote(string note)
        {
            txtNote.Clear();
            txtNote.SendKeys(note);
        }

        public void SelectPayment(string value)
        {
            new SelectElement(ddlPayment).SelectByText(value);
        }

        public void SelectStatus(string value)
        {
            new SelectElement(ddlStatus).SelectByText(value);
        }

        public void ClickSave()
        {
            btnSave.Click();
        }

        public void ClickCancel()
        {
            btnCancel.Click();
        }
    }
}