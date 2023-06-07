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
public class GenresTests
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

        [TestCase("ACTION", "Action games involve high levels of physical challenges, including combat, exploration, and problem-solving.")]
        [TestCase("ADVENTURE", "Adventure games focus on exploration, storytelling, and puzzle-solving in a fictional or fantastical setting.")]
        // Додайте інші випадки для інших жанрів
        public void GetGenreDescription_WhenValidGenre_ReturnsDescription(string genreName, string expectedDescription)
        {
            // Act
            var result = _controller.GetGenreDescription(genreName);

            // Assert
            Assert.AreEqual(expectedDescription, result);
        }

        [Test]
        public void GetGenreDescription_WhenInvalidGenre_ReturnsDefaultDescription()
        {
            // Arrange
            var invalidGenre = "INVALID";

            // Act
            var result = _controller.GetGenreDescription(invalidGenre);

            // Assert
            Assert.AreEqual("No description available.", result);
        }
}
