using KhachSanTest.Pages;
using KhachSanTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Linq;
using System.Threading;

namespace KhachSanTest.Tests
{
    public class RolePermissionTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        RolePermissionPage rolePage;

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetPhanQuyenTestCases))]
        public void Test_RolePermission(ExcelDataProvider.TestCase tc)
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            loginPage = new LoginPage(driver);
            rolePage = new RolePermissionPage(driver);

            string actual = "";
            string status = "Passed";
            string imagePath = "";

            try
            {
                foreach (var step in tc.Steps)
                {
                    ExecuteStep(step);
                }

                string expected = GetExpectedResult(tc) ?? "";
                string actualResult = SafeGetActualResult();

                actual = actualResult;

                bool isMatch = CompareResult(expected, actualResult);
                status = isMatch ? "Passed" : "Failed";

                Assert.That(isMatch, Is.True, $"Sai kết quả tại {tc.TestCaseId}");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = ex.Message;
                imagePath = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(
                    tc.SheetName,
                    tc.TestCaseId,
                    actual,
                    status,
                    imagePath
                );
            }
        }

        // ================= STEP =================
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action))
                return;

            string action = step.Action.ToLower();
            string data = step.Data ?? "";

            // ===== LOGIN =====
            if (action.Contains("nhập username") && data == "admin")
            {
                loginPage.EnterUsername(data);
            }
            else if (action.Contains("nhập username") && data == "user")
            {
                loginPage.EnterUsername(data);
            }
            else if (action.Contains("nhập password"))
            {
                loginPage.EnterPassword(data);
            }
            else if (action.Contains("nhấn đăng nhập"))
            {
                loginPage.ClickLogin();
                Thread.Sleep(1000);
            }

            // ===== VÀO USER MANAGEMENT =====
            else if (action.Contains("vào trang quản trị"))
            {
                rolePage.GoToUserManagement();
                Thread.Sleep(1000);
            }

            // ===== CHỌN USER =====
            else if (action.Contains("chọn tài khoản"))
            {
                rolePage.SelectUser(data);
            }

            // ===== EDIT =====
            else if (action.Contains("nhấn sửa"))
            {
                rolePage.ClickEdit();
            }

            // ===== ROLE =====
            else if (action.Contains("chọn roleid"))
            {
                rolePage.SelectRole(data);
            }
            else if (action.Contains("không chọn roleid"))
            {
                rolePage.ClearRole();
            }

            // ===== SAVE =====
            else if (action.Contains("lưu thay đổi"))
            {
                rolePage.ClickSave();
                Thread.Sleep(1500);
            }

            // ===== DROPDOWN =====
            else if (action.Contains("mở dropdown"))
            {
                rolePage.OpenRoleDropdown();
            }
        }

        // ================= EXPECTED =================
        private string GetExpectedResult(ExcelDataProvider.TestCase tc)
        {
            return tc.Steps
                .Where(s => !string.IsNullOrEmpty(s.Expected))
                .LastOrDefault()?.Expected?.Trim();
        }

        // ================= ACTUAL =================
        private string SafeGetActualResult()
        {
            try
            {
                string url = driver.Url.ToLower();

                if (url.Contains("/users"))
                    return "Cập nhật quyền thành công";

                var errors = driver.FindElements(By.ClassName("text-danger"));
                if (errors.Count > 0)
                    return errors[0].Text;

                var msg = driver.FindElements(By.ClassName("alert"));
                if (msg.Count > 0)
                    return msg[0].Text;

                return "";
            }
            catch
            {
                return "Lỗi hệ thống";
            }
        }

        // ================= COMPARE =================
        private bool CompareResult(string expected, string actual)
        {
            string e = expected.ToLower();
            string a = actual.ToLower();

            if (e.Contains("thành công"))
                return a.Contains("thành công");

            if (e.Contains("lỗi"))
                return a.Contains("lỗi") || a.Contains("error");

            if (e.Contains("không cho phép"))
                return a.Contains("không") || a.Contains("denied");

            if (e.Contains("chưa chọn"))
                return a.Contains("chọn");

            if (e.Contains("không có thay đổi"))
                return a.Contains("không") || a.Contains("no change");

            return a.Contains(e);
        }

        // ================= TEARDOWN =================
        [TearDown]
        public void Cleanup()
        {
            driver?.Quit();
            driver?.Dispose();
            driver = null;
        }
    }
}