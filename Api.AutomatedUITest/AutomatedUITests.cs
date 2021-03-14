using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Api.AutomatedUITest
{
    public class AutomatedUITests : IDisposable
    {
        private readonly IWebDriver _driver;
        public AutomatedUITests()
        {
            _driver = new FirefoxDriver();
        }

        [Fact]
        public void Create_WhenExecuted_ReturnsCreateView()
        {
            _driver.Navigate().GoToUrl("http://localhost:5555/api/v1/posts");
            Assert.Equal("Create - EmployeesApp", _driver.Title);
            Assert.Contains("Please provide a new employee data", _driver.PageSource);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
