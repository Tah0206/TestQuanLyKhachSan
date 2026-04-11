using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KhachSanTest.Pages
{
    public class RolePermissionPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        // Username của user đang được chọn, dùng để tìm đúng nút Edit
        private string _selectedUsername = "";

        public RolePermissionPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // ===== LOCATOR =====
        // Index.cshtml: <a href=".../Edit/..." class="btn-edit-user">
        // Edit.cshtml:  <button type="submit" class="btn-delete">
        // Edit.cshtml:  <select id="RoleId" ...>

        private By userRows = By.CssSelector("table tbody tr");
        // FIX 1: class đúng trong HTML là "btn-edit-user" (thẻ <a>)
        private By btnEditUser = By.CssSelector("a.btn-edit-user");
        private By ddlRole = By.Id("RoleId");
        // FIX 2: class đúng trong HTML là "btn-delete" (button submit trên Edit page)
        private By btnSave = By.CssSelector("button.btn-delete");

        // ===== ACTION =====

        public void GoToUserManagement()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/Users");
            wait.Until(d => d.Url.ToLower().Contains("/users"));
        }

        /// <summary>
        /// Ghi nhớ username để ClickEdit() tìm đúng nút Sửa trong hàng đó.
        /// Không click vào <tr> vì HTML không có event click trên row.
        /// </summary>
        public void SelectUser(string username)
        {
            _selectedUsername = username.Trim().ToLower();

            // Chờ bảng load
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(userRows));
        }

        /// <summary>
        /// Tìm hàng chứa username đã chọn rồi click nút "btn-edit-user" trong hàng đó.
        /// Sau đó chờ dropdown #RoleId visible để đảm bảo trang Edit đã load hoàn toàn.
        /// </summary>
        public void ClickEdit()
        {
            // Đảm bảo đang ở trang danh sách trước khi tìm row
            wait.Until(d => d.Url.ToLower().Contains("/users") && !d.Url.ToLower().Contains("/edit"));

            IReadOnlyList<IWebElement> rows =
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(userRows));

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells.Count == 0) continue;

                string cellText = cells[0].Text.Trim().ToLower();
                if (cellText == _selectedUsername || cellText.Contains(_selectedUsername))
                {
                    var editLink = row.FindElement(By.CssSelector("a.btn-edit-user"));
                    editLink.Click();

                    // Chờ URL chuyển sang /Edit/
                    wait.Until(d => d.Url.ToLower().Contains("/edit"));

                    // QUAN TRỌNG: chờ #RoleId thực sự hiển thị trên trang Edit
                    // Nếu không có bước này, SelectRole() sẽ bị timeout vì trang chưa render xong
                    wait.Until(ExpectedConditions.ElementIsVisible(ddlRole));
                    return;
                }
            }

            throw new NoSuchElementException(
                $"Không tìm thấy user '{_selectedUsername}' trong bảng danh sách.");
        }

        public void SelectRole(string role)
        {
            // Dùng ElementIsVisible thay vì ElementExists để chắc chắn dropdown đã render
            var ddlElement = wait.Until(ExpectedConditions.ElementIsVisible(ddlRole));
            var select = new SelectElement(ddlElement);

            foreach (var option in select.Options)
            {
                if (option.Text.Trim().ToLower() == role.Trim().ToLower())
                {
                    option.Click();
                    return;
                }
            }

            // Nếu không khớp exact, thử contains (đề phòng có khoảng trắng thừa hoặc encoding)
            foreach (var option in select.Options)
            {
                if (option.Text.Trim().ToLower().Contains(role.Trim().ToLower()))
                {
                    option.Click();
                    return;
                }
            }

            string availableOptions = string.Join(", ", select.Options
                .Select(o => $"'{o.Text.Trim()}'"));
            throw new NoSuchElementException(
                $"Không tìm thấy role '{role}' trong dropdown. Các option hiện có: {availableOptions}");
        }

        public void ClearRole()
        {
            var ddlElement = wait.Until(ExpectedConditions.ElementIsVisible(ddlRole));
            var select = new SelectElement(ddlElement);
            select.SelectByIndex(0);
        }

        public void OpenRoleDropdown()
        {
            var ddl = wait.Until(ExpectedConditions.ElementIsVisible(ddlRole));
            ddl.Click();
        }

        /// <summary>
        /// FIX: Nút submit trên Edit.cshtml có class "btn-delete" (không phải "btn-save").
        /// </summary>
        public void ClickSave()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(btnSave));
            btn.Click();
        }
    }
}