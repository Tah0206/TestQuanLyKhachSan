using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KhachSanTest.Pages
{
    public class SearchUserPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public SearchUserPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // ✅ Sửa: dùng name="search" đúng với Index.cshtml (không phải id="searchString")
        private By txtSearch = By.CssSelector("input[name='search']");
        private By btnSearch = By.CssSelector("button[type='submit']");

        // Các dòng trong bảng kết quả
        private By tableRows = By.CssSelector("table.user-table tbody tr");

        public void GoToUsers()
        {
            driver.Navigate().GoToUrl("http://localhost:58609/Users");
            wait.Until(ExpectedConditions.ElementIsVisible(txtSearch));
        }

        public void EnterSearch(string value)
        {
            var e = wait.Until(ExpectedConditions.ElementIsVisible(txtSearch));
            e.Clear();
            e.SendKeys(value ?? "");
        }

        public void ClickSearch()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnSearch)).Click();
            // Chờ trang reload xong (URL có query string search=...)
            wait.Until(d => d.Url.Contains("search=") || d.Url.Contains("/Users"));
        }

        /// <summary>
        /// Lấy kết quả thực tế sau khi tìm kiếm.
        /// Trả về: "Tìm kiếm thành công: N kết quả" hoặc "Không tìm thấy kết quả"
        /// </summary>
        public string GetSearchResult(string keyword)
        {
            try
            {
                // Chờ bảng render xong
                System.Threading.Thread.Sleep(1000);

                var rows = driver.FindElements(tableRows);

                if (rows.Count == 0)
                    return "Không tìm thấy kết quả";

                // Nếu có keyword, kiểm tra ít nhất 1 dòng chứa keyword đó
                if (!string.IsNullOrEmpty(keyword))
                {
                    bool hasMatch = rows.Any(r =>
                        r.Text.ToLower().Contains(keyword.ToLower()));

                    return hasMatch
                        ? $"Tìm kiếm thành công: {rows.Count} kết quả"
                        : "Không tìm thấy kết quả";
                }

                return $"Tìm kiếm thành công: {rows.Count} kết quả";
            }
            catch
            {
                return "Lỗi khi lấy kết quả";
            }
        }
    }
}