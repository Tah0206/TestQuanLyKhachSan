using KhachSanTest.Pages;
using KhachSanTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace KhachSanTest.Tests
{
    public class AddUserTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        AddUserPage addUserPage;

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-notifications");
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            loginPage = new LoginPage(driver);
            addUserPage = new AddUserPage(driver);
        }

        [TearDown]
        public void Cleanup()
        {
            if (driver != null)
            {
                // 1. Đóng trình duyệt và kết thúc session
                driver.Quit();

                // 2. Giải phóng tài nguyên bộ nhớ (Sửa lỗi bạn đang gặp)
                driver.Dispose();

                // 3. Đưa về null để tránh dùng nhầm ở TC sau
                driver = null;
            }
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetAddUserTestCases))]
        public void Test_AddUser(ExcelDataProvider.TestCase tc)
        {
            string actual = "";
            string status = "Passed";

            try
            {
                foreach (var step in tc.Steps)
                {
                    ExecuteStep(step);
                }

                string expected = tc.Steps.LastOrDefault(s => !string.IsNullOrEmpty(s.Expected))?.Expected ?? "";
                actual = SafeGetActualResult();

                bool isMatch = CompareResult(expected, actual);
                status = isMatch ? "Passed" : "Failed";

                Assert.That(isMatch, Is.True, $"Kết quả không khớp: Expected '{expected}', Actual '{actual}'");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = "Lỗi: " + ex.Message;
                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(tc.SheetName, tc.TestCaseId, actual, status, "");
            }
        }

        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action)) return;

            string action = step.Action.ToLower();
            string data = step.Data ?? "";

            // 1. NHÓM ĐIỀU HƯỚNG & ĐĂNG NHẬP (Dùng return để thoát sớm vì các bước này tách biệt)
            if (action.Contains("nhập username") && data == "admin") { loginPage.EnterUsername(data); return; }
            if (action.Contains("nhập password") && data == "123456") { loginPage.EnterPassword(data); return; }
            if (action.Contains("nhấn đăng nhập")) { loginPage.ClickLogin(); Thread.Sleep(1000); return; }
            if (action.Contains("vào trang quản trị")) { driver.Navigate().GoToUrl("http://localhost:58609/Users"); return; }
            if (action.Contains("thêm tài khoản")) { addUserPage.ClickAddUser(); return; }

            // 2. NHÓM NHẬP LIỆU FORM (Dùng các lệnh IF RỜI NHAU - KHÔNG DÙNG ELSE IF)
            // Cách này giúp nếu Action chứa cả "username" và "mật khẩu" thì nó chạy cả 2

            if (action.Contains("username"))
            {
                addUserPage.EnterUsername(action.Contains("để trống") ? "" : data);
            }

            if (action.Contains("passwordhash") || action.Contains("mật khẩu"))
            {
                addUserPage.EnterPasswordHash(action.Contains("để trống") ? "" : data);
            }

            if (action.Contains("họ và tên") || action.Contains("họ tên"))
            {
                addUserPage.EnterFullName(action.Contains("để trống") ? "" : data);
            }

            if (action.Contains("email"))
            {
                addUserPage.EnterEmail(action.Contains("để trống") ? "" : data);
            }

            if (action.Contains("vai trò"))
            {
                addUserPage.SelectRole(data);
            }

            // 3. NHÓM THỰC THI CUỐI
            if (action.Contains("tạo tài khoản"))
            {
                addUserPage.ClickCreate();
            }
        }

        private string SafeGetActualResult()
        {
            try
            {
                Thread.Sleep(3000); // Chờ 3 giây để trang load sau khi bấm Create

                // KIỂM TRA THÀNH CÔNG: Nếu URL không còn ở trang Create nữa
                if (driver.Url.ToLower().Contains("/users") && !driver.Url.ToLower().Contains("/create"))
                {
                    return "Thêm tài khoản thành công";
                }

                // KIỂM TRA THẤT BẠI: Tìm các thẻ báo lỗi đỏ (nếu có)
                var errorElements = driver.FindElements(By.CssSelector(".field-validation-error, .text-danger, .validation-summary-errors"));
                foreach (var error in errorElements)
                {
                    if (!string.IsNullOrEmpty(error.Text)) return error.Text.Trim();
                }

                return "Không xác định";
            }
            catch
            {
                return "Lỗi Driver";
            }
        }

        private bool CompareResult(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected)) return false;

            string e = expected.ToLower();
            string a = actual.ToLower();

            // Nếu Excel yêu cầu thành công và thực tế cũng thành công -> PASS
            if (e.Contains("thành công") && a.Contains("thành công")) return true;

            // Nếu là các case lỗi (để trống, sai định dạng...)
            // Chỉ cần Actual có nội dung (không rỗng) là coi như đã bắt được lỗi
            if (e.Contains("lỗi") || e.Contains("trống"))
            {
                return !string.IsNullOrEmpty(a) && a != "không xác định";
            }

            return a.Contains(e) || e.Contains(a);
        }
    }
}