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

                // FIX: Chụp màn hình ngay khi kết quả KHÔNG khớp (trước khi Assert ném exception)
                if (!isMatch)
                {
                    imagePath = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                }

                Assert.That(isMatch, Is.True,
                    $"[{tc.TestCaseId}] Expected: '{expected}' | Actual: '{actualResult}'");
            }
            catch (Exception ex) when (!(ex is AssertionException))
            {
                // Exception không phải do Assert (vd: NoSuchElement, Timeout...)
                status = "Failed";
                actual = ex.Message;

                // Chỉ chụp nếu chưa chụp ở trên
                if (string.IsNullOrEmpty(imagePath))
                    imagePath = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);

                throw;
            }
            catch (AssertionException)
            {
                // Assert thất bại: ảnh đã được chụp ở trên, chỉ cần re-throw
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
            if (action.Contains("nhập username"))
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
            // FIX: SelectUser chỉ ghi nhớ username; ClickEdit() mới thực sự click nút Sửa
            else if (action.Contains("nhấn sửa"))
            {
                rolePage.ClickEdit();
                Thread.Sleep(1000);
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

                // Sau khi lưu thành công -> redirect về /Users
                if (url.Contains("/users") && !url.Contains("/edit"))
                    return "Cập nhật quyền thành công";

                // Đang ở trang Edit → đọc các option trong dropdown #RoleId
                // Dùng cho test case kiểm tra "dropdown hiển thị đủ N quyền"
                if (url.Contains("/edit"))
                {
                    var ddlElems = driver.FindElements(By.Id("RoleId"));
                    if (ddlElems.Count > 0)
                    {
                        var select = new OpenQA.Selenium.Support.UI.SelectElement(ddlElems[0]);
                        var options = select.Options
                            .Where(o => !string.IsNullOrWhiteSpace(o.Text.Trim()))
                            .Select(o => o.Text.Trim())
                            .ToList();

                        if (options.Count > 0)
                            return "Hiển thị " + string.Join(", ", options);
                    }
                }

                // Lỗi validation
                var errors = driver.FindElements(By.ClassName("text-danger"));
                foreach (var err in errors)
                {
                    string txt = err.Text?.Trim();
                    if (!string.IsNullOrEmpty(txt))
                        return txt;
                }

                // Alert thông báo
                var alerts = driver.FindElements(By.ClassName("alert"));
                if (alerts.Count > 0)
                    return alerts[0].Text?.Trim();

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
            if (string.IsNullOrEmpty(expected)) return true;

            string e = expected.ToLower().Trim();
            string a = actual.ToLower().Trim();

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

            // So sánh danh sách quyền trong dropdown
            // Expected: "Hiển thị Admin, Nhân viên, Khách hàng, Quản lý"
            // Actual:   "Hiển thị Admin, Nhân viên, Khách hàng, Quản lý"
            if (e.Contains("hiển thị"))
            {
                string eList = e.Replace("hiển thị", "").Trim();
                string aList = a.Replace("hiển thị", "").Trim();

                var expectedItems = eList.Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                // Tất cả quyền trong expected phải xuất hiện trong actual
                return expectedItems.All(item => aList.Contains(item));
            }

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