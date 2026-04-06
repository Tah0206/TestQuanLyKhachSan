using KhachSanTest.Pages;
using KhachSanTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace KhachSanTest.Tests
{
    public class EditUserTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        EditUserPage editPage;
        WebDriverWait wait;

        string currentUser = ""; // 🔥 lưu user đang thao tác

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            loginPage = new LoginPage(driver);
            editPage = new EditUserPage(driver);
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetEditUserTestCases))]
        public void Test_EditUser(ExcelDataProvider.TestCase tc)
        {
            string actual = "";
            string status = "Passed";

            try
            {
                foreach (var step in tc.Steps)
                {
                    ExecuteStep(step);
                }

                actual = GetActual();
                string expected = GetExpected(tc);

                bool isMatch = CompareResult(expected, actual);
                status = isMatch ? "Passed" : "Failed";

                Assert.That(isMatch, Is.True, $"❌ Sai TC: {tc.TestCaseId}");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = ex.Message;
                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(tc.SheetName, tc.TestCaseId, actual, status, "");
            }
        }

        // ================= STEP =================
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            string action = step.Action.ToLower();
            string data = step.Data ?? "";

            // ===== LOGIN =====
            if (action.Contains("username") && data == "admin")
                loginPage.EnterUsername(data);

            else if (action.Contains("password") && data == "123456")
                loginPage.EnterPassword(data);

            else if (action.Contains("đăng nhập"))
            {
                loginPage.ClickLogin();
                wait.Until(d => !d.Url.Contains("Login"));
            }

            // ===== NAV =====
            else if (action.Contains("vào trang"))
                editPage.GoToUsers();

            // ===== CHỌN USER =====
            else if (action.Contains("chọn tài khoản"))
            {
                currentUser = data; // 🔥 lưu lại user
            }

            // ===== CLICK EDIT (QUAN TRỌNG NHẤT) =====
            else if (action.Contains("nhấn nút sửa"))
            {
                if (string.IsNullOrEmpty(currentUser))
                    throw new Exception("Chưa chọn user để sửa");

                editPage.ClickEditByUsername(currentUser);
            }

            // ===== INPUT =====
            else if (action.Contains("nhập username"))
                editPage.EnterUsername(data);

            else if (action.Contains("passwordhash"))
                editPage.EnterPassword(data);

            else if (action.Contains("họ tên"))
                editPage.EnterFullName(data);

            else if (action.Contains("nhập email"))
                editPage.EnterEmail(data);

            else if (action.Contains("chọn role") || action.Contains("đổi role"))
                editPage.SelectRole(data);

            // ===== CLEAR =====
            else if (action.Contains("xóa họ tên"))
                editPage.EnterFullName("");

            else if (action.Contains("xóa email"))
                editPage.EnterEmail("");

            // ===== SAVE =====
            else if (action.Contains("lưu"))
                editPage.ClickSave();

            else if (action.Contains("quay lại"))
                editPage.ClickBack();
        }

        // ================= EXPECTED =================
        private string GetExpected(ExcelDataProvider.TestCase tc)
        {
            return tc.Steps
                .Where(s => !string.IsNullOrEmpty(s.Expected))
                .LastOrDefault()?.Expected ?? "";
        }

        // ================= ACTUAL =================
        private string GetActual()
        {
            try
            {
                var errors = driver.FindElements(By.ClassName("text-danger"));
                if (errors.Count > 0 && !string.IsNullOrEmpty(errors[0].Text))
                    return errors[0].Text;

                var alert = driver.FindElements(By.ClassName("alert"));
                if (alert.Count > 0)
                    return alert[0].Text;

                return "Cập nhật thành công";
            }
            catch
            {
                return "";
            }
        }

        // ================= COMPARE =================
        private bool CompareResult(string expected, string actual)
        {
            string e = expected.ToLower();
            string a = actual.ToLower();

            if (e.Contains("thành công"))
                return a.Contains("thành công");

            if (e.Contains("họ tên"))
                return a.Contains("tên") || a.Contains("name");

            if (e.Contains("email"))
                return a.Contains("email");

            if (e.Contains("không hợp lệ"))
                return a.Contains("invalid") || a.Contains("không");

            if (e.Contains("vui lòng"))
                return a.Contains("vui lòng");

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