using OpenQA.Selenium;
using System;
using System.IO;

namespace KhachSanTest.Utilities
{
    public static class ScreenshotHelper
    {
        public static string TakeScreenshot(IWebDriver driver, string testName)
        {
            try
            {
                Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();

                string folder = @"D:\Code\Code Web\N3K2\Testter\KhachSanTest\Image";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = testName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

                string fullPath = Path.Combine(folder, fileName);

                ss.SaveAsFile(fullPath);

                return fullPath;
            }
            catch
            {
                return "Cannot capture screenshot";
            }
        }
    }
}