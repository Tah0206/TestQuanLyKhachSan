using KhachSanTest.Pages;
using KhachSanTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace KhachSanTest.Tests
{
    public class SearchUserTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        SearchUserPage searchPage;
        WebDriverWait wait;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            loginPage = new LoginPage(driver);
            searchPage = new SearchUserPage(driver);
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetSearchUserTestCases))]
        public void Test_SearchUser(ExcelDataProvider.TestCase tc)
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
                searchPage.GoToUsers();

            else if (action.Contains("nhập"))
                searchPage.EnterSearch(step.Data);

            else if (action.Contains("tìm kiếm"))
                searchPage.ClickSearch();
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