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
    public class DeleteUserTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        DeleteUserPage deletePage;
        WebDriverWait wait;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            loginPage = new LoginPage(driver);
            deletePage = new DeleteUserPage(driver);
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetDeleteUserTestCases))]
        public void Test_DeleteUser(ExcelDataProvider.TestCase tc)
        {
            foreach (var step in tc.Steps)
            {
                ExecuteStep(step);
            }

            Assert.Pass();
        }

        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            string action = step.Action.ToLower();

            if (action.Contains("username") && step.Data == "admin")
                loginPage.EnterUsername(step.Data);

            else if (action.Contains("password"))
                loginPage.EnterPassword(step.Data);

            else if (action.Contains("đăng nhập"))
                loginPage.ClickLogin();

            else if (action.Contains("vào trang"))
                deletePage.GoToUsers();

            else if (action.Contains("chọn tài khoản"))
                deletePage.SelectUser(step.Data);

            else if (action.Contains("xóa"))
                deletePage.ClickDelete();

            else if (action.Contains("xác nhận"))
                deletePage.ConfirmDelete();

            else if (action.Contains("hủy"))
                deletePage.CancelDelete();
        }

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