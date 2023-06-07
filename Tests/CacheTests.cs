using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PlayAndConnect.Controllers;
using PlayAndConnect.Data;
using PlayAndConnect.Data.Interfaces;
using PlayAndConnect.Tests;
using Microsoft.Extensions.FileProviders;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;



[TestFixture]
public class CacheTests
{
    private ApplicationDbContext _dbContext;
    private IGameDb _gameDb;
    private IUserDb _userDb;
    private IInfoDb _infoDb;
    private IHttpContextAccessor _httpContextAccessor;
    private IWebHostEnvironment _webHostEnvironment;
    private HomeController _controller;

    [SetUp]
    public void Setup()
    {
        // Налаштування віртуальної бази даних
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _gameDb = new GameDb(_dbContext);
        _userDb = new UserDb(_dbContext);
        _infoDb = new InfoDb(_dbContext);
        _httpContextAccessor = new HttpContextAccessor();
        _webHostEnvironment = new MockWebHostEnvironment();

        // Виріште, якщо потрібно впровадити додаткові залежності для контролера

        _controller = new HomeController(_dbContext, _gameDb, _userDb, _infoDb, _httpContextAccessor, _webHostEnvironment);
    }

             [Test]
        public void GetLoginFromCookie_WhenUserLoggedIn_ReturnsTrueAndSetsLogin()
        {
            // Arrange
            var user = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "john.doe")
                })
            );

            _httpContextAccessor.HttpContext = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.User = user;

            // Act
            var result = _controller.GetLoginFromCookie(out var login);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("john.doe", login);
        }

        [Test]
        public void GetLoginFromCookie_WhenUserNotLoggedIn_ReturnsFalseAndSetsLoginToNull()
        {
            // Arrange
            _httpContextAccessor.HttpContext = new DefaultHttpContext();

            // Act
            var result = _controller.GetLoginFromCookie(out var login);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(login);
        }

        [Test]
        public void IsUserAuthorized_WhenUserLoggedIn_ReturnsTrue()
        {
            // Arrange
            var user = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "john.doe")
                })
            );

            _httpContextAccessor.HttpContext = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.User = user;

            // Act
            var result = _controller.isUserAutorized();//isUserAuthorized();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsUserAuthorized_WhenUserNotLoggedIn_ReturnsFalse()
        {
            // Arrange
            _httpContextAccessor.HttpContext = new DefaultHttpContext();

            // Act
            var result = _controller.isUserAutorized();

            // Assert
            Assert.IsFalse(result);
        }
}
