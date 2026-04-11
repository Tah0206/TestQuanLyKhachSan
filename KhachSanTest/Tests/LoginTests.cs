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
    public class LoginTests
    {
        IWebDriver driver;
        LoginPage loginPage;

        // ✅ FLAG CHỈ DÙNG CHO TEST ADMIN
        bool isAccessingAdmin = false;

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetLoginTestCases))]
        public void Test_Login(ExcelDataProvider.TestCase tc)
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            loginPage = new LoginPage(driver);

            TestContext.WriteLine($"\n===== RUN TC: {tc.TestCaseId} =====");

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

                TestContext.WriteLine($"Expected: {expected}");
                TestContext.WriteLine($"Actual: {actualResult}");

                bool isMatch = CompareResult(expected, actualResult);

                status = isMatch ? "Passed" : "Failed";

                TestContext.WriteLine($"Status: {status}");

                Assert.That(isMatch, Is.True, $"Sai kết quả tại {tc.TestCaseId}");
            }
            catch (AssertionException)
            {
                status = "Failed";
                imagePath = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                TestContext.WriteLine($"FAIL: {tc.TestCaseId}");
                throw;
            }
            catch (Exception ex)
            {
                status = "Failed";

                actual = ex.Message.Contains("stale element")
                    ? "Lỗi hệ thống: stale element"
                    : ex.Message;

                imagePath = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);

                TestContext.WriteLine($"ERROR: {actual}");
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

                // ✅ RESET FLAG
                isAccessingAdmin = false;
            }
        }

        // ================= SO SÁNH =================
        private bool CompareResult(string expected, string actual)
        {
            string e = expected.Trim().ToLower();
            string a = actual.Trim().ToLower();

            if (a.Contains("stale element"))
                return false;

            if (e.Contains("đăng nhập thành công"))
                return a.Contains("đăng nhập thành công");

            if (e.Contains("vào trang user") || e.Contains("vào trang admin"))
                return a.Contains("đăng nhập thành công");

            if (e.Contains("hiển thị lỗi mật khẩu") || e.Contains("lỗi mật khẩu hoặc đăng nhập"))
                return a.Contains("sai") || a.Contains("mật khẩu") || a.Contains("đăng nhập");

            if (e.Contains("sai mật khẩu"))
                return a.Contains("sai") || a.Contains("mật khẩu");

            if (e.Contains("tài khoản không tồn tại"))
                return a.Contains("không tồn tại") || a.Contains("sai");

            if (e.Contains("lỗi username"))
                return a.Contains("fill") || a.Contains("username");

            if (e.Contains("lỗi password"))
                return a.Contains("fill") || a.Contains("password");

            if (e.Contains("lỗi 2 trường"))
                return a.Contains("fill");

            // ✅ CASE CHẶN ADMIN
            if (e.Contains("chặn") || e.Contains("không cho vào trang admin"))
                return a.Contains("chặn") || a.Contains("không cho") || a.Contains("login");

            if (e.Contains("nhớ mật khẩu") || e.Contains("remember"))
                return a.Contains("không có chức năng");

            return a.Contains(e);
        }

        // ================= LẤY ACTUAL =================
        private string SafeGetActualResult()
        {
            try
            {
                string url = driver.Url.ToLower();

                // ✅ CHỈ CHECK ADMIN KHI ĐANG TEST ADMIN
                if (isAccessingAdmin)
                {
                    if (url.Contains("login"))
                    {
                        return "Chặn không cho vào trang admin";
                    }

                    if (driver.PageSource.ToLower().Contains("không có quyền") ||
                        driver.PageSource.ToLower().Contains("access denied"))
                    {
                        return "Chặn không cho vào trang admin";
                    }

                    return "Đăng nhập thành công";
                }

                // ================= LOGIN BÌNH THƯỜNG =================

                if (!url.Contains("login"))
                {
                    return "Đăng nhập thành công";
                }

                var errors = driver.FindElements(By.ClassName("text-danger"));
                if (errors.Count > 0 && !string.IsNullOrEmpty(errors[0].Text))
                {
                    return errors[0].Text.Trim();
                }

                var validations = driver.FindElements(By.CssSelector(".field-validation-error"));
                if (validations.Count > 0 && !string.IsNullOrEmpty(validations[0].Text))
                {
                    return validations[0].Text.Trim();
                }

                var inputs = driver.FindElements(By.CssSelector("input:invalid"));
                if (inputs.Count > 0)
                {
                    return inputs[0].GetAttribute("validationMessage");
                }

                return "";
            }
            catch (StaleElementReferenceException)
            {
                return "Lỗi hệ thống: stale element";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // ================= STEP =================
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action))
                return;

            string action = step.Action.ToLower();

            if (action.Contains("vào trang"))
            {
                driver.Navigate().GoToUrl("http://localhost:58609/");
            }
            else if (action.Contains("username") && !action.Contains("trống"))
            {
                loginPage.EnterUsername(step.Data ?? "");
            }
            else if (action.Contains("password") && !action.Contains("trống"))
            {
                loginPage.EnterPassword(step.Data ?? "");
            }
            else if (action.Contains("để trống username"))
            {
                loginPage.EnterUsername("");
            }
            else if (action.Contains("để trống password"))
            {
                loginPage.EnterPassword("");
            }
            else if (action.Contains("remember"))
            {
                var checkbox = driver.FindElements(By.Id("RememberMe"));

                if (checkbox.Count == 0)
                {
                    throw new Exception("Không có chức năng Remember Me");
                }

                loginPage.ClickRememberMe();
            }
            else if (action.Contains("nhấn đăng nhập"))
            {
                loginPage.ClickLogin();
                Thread.Sleep(1000);
            }
            // ✅ TRUY CẬP URL (ADMIN TEST)
            else if (action.Contains("dùng đường dẫn") || action.Contains("truy cập"))
            {
                if (!string.IsNullOrEmpty(step.Data))
                {
                    driver.Navigate().GoToUrl(step.Data);
                    Thread.Sleep(1000);

                    if (step.Data.ToLower().Contains("tongquan"))
                    {
                        isAccessingAdmin = true;
                    }
                }
            }
            else if (action.Contains("không thao tác"))
            {
                Thread.Sleep(2000);
            }
        }

        // ================= EXPECTED =================
        private string GetExpectedResult(ExcelDataProvider.TestCase tc)
        {
            return tc.Steps
                .Where(s => !string.IsNullOrEmpty(s.Expected))
                .LastOrDefault()?.Expected?.Trim();
        }

        // ================= TEARDOWN =================
        [TearDown]
        public void Cleanup()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
                driver = null;
            }
        }
    }
}