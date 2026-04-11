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
        // Dùng By thay vì property trực tiếp để tránh StaleElementReferenceException

        private By ddlCustomer = By.Name("CustomerId");
        private By ddlRoom = By.Name("RoomId");
        private By txtCheckIn = By.Name("CheckInDate");
        private By txtCheckOut = By.Name("CheckOutDate");
        private By txtPeople = By.Name("NumberOfPeople");
        private By txtNote = By.Name("Note");
        private By ddlPayment = By.Name("PaymentStatusID");
        private By ddlStatus = By.Name("BookingStatusID");

        // Trong Edit.cshtml: <input type="submit" value="Save" class="btn btn-default" />
        private By btnSave = By.CssSelector("input[type='submit'][value='Save']");

        // Trong Edit.cshtml: @Html.ActionLink("Back to List", "Index") → <a href="...">Back to List</a>
        private By btnCancel = By.LinkText("Back to List");

        // ===== HELPER =====
        private IWebElement WaitFor(By by, int seconds = 10)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            return wait.Until(d => d.FindElement(by));
        }

        // ===== ACTION =====
        public void SelectCustomer(string name)
        {
            new SelectElement(WaitFor(ddlCustomer)).SelectByText(name);
        }

        public void SelectRoom(string room)
        {
            new SelectElement(WaitFor(ddlRoom)).SelectByText(room);
        }

        public void EnterCheckIn(string date)
        {
            var el = WaitFor(txtCheckIn);
            el.Clear();
            el.SendKeys(date);
        }

        public void EnterCheckOut(string date)
        {
            var el = WaitFor(txtCheckOut);
            el.Clear();
            el.SendKeys(date);
        }

        public void EnterPeople(string people)
        {
            var el = WaitFor(txtPeople);
            el.Clear();
            el.SendKeys(people);
        }

        public void EnterNote(string note)
        {
            var el = WaitFor(txtNote);
            el.Clear();
            el.SendKeys(note);
        }

        public void SelectPayment(string value)
        {
            new SelectElement(WaitFor(ddlPayment)).SelectByText(value);
        }

        public void SelectStatus(string value)
        {
            new SelectElement(WaitFor(ddlStatus)).SelectByText(value);
        }

        public void ClickSave()
        {
            WaitFor(btnSave).Click();
        }

        public void ClickCancel()
        {
            WaitFor(btnCancel).Click();
        }
    }
}