using System.Text.RegularExpressions;
using System.Security.AccessControl;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PlayAndConnect.Models;
using System.Diagnostics;
using PlayAndConnect.Data;
using PlayAndConnect.Data.Interfaces;
using PlayAndConnect.ViewModels;
using System.Security.Claims;
using Pomelo.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace PlayAndConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserDb _userDb;
        public HomeController(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, IUserDb userDb)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userDb = userDb;
        }


        [HttpGet]
        [Authorize]
        public IActionResult AddGame()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            Genre? cheak = _db.Genres.FirstOrDefault<Genre>(g => g.Id == 3);
            if (cheak == null)
            {
                return RedirectToAction("AddGenres");
            }
            SelectList genres = new SelectList(_db.Genres, "Id", "Name");
            ViewBag.GenresList = genres;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddGame(int selectGenre, string title = "default", string description = "default")
        {
            Game newGame = new();
            Genre? genre = _db.Genres.FirstOrDefault<Genre>(g => g.Id == selectGenre);
            newGame.Description = description;
            newGame.Title = title;
            if (genre != null)
                newGame.Genre = genre;
            else
                newGame.Genre = _db.Genres.FirstOrDefault<Genre>(g => g.Id == 3);
            _db.Games.Add(newGame);
            _db.SaveChanges();
            return RedirectToAction("AddGame");
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            await Logout();
            if (ModelState.IsValid)
            {
                User? user = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == username && u.PasswordHash == Hashing.HashPassword(password));
                if (user != null)
                {
                    await Authenticate(user.Login);
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Логін або пароль неправильні";
                    return RedirectToAction("Login");
                }
            }
            else
                return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Signup()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            if (TempData.ContainsKey("username"))
            {
                ViewBag.username = TempData["username"];
                TempData.Remove("username");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(string username, string password, string confirm_password)
        {
            await Logout();
            if (ModelState.IsValid)
            {
                if (password != confirm_password)
                {
                    TempData["username"] = username;
                    TempData["Error"] = "Паролі не співпадають";
                    return RedirectToAction("Signup");
                }
                User? user = _db.Users.FirstOrDefault<User>(u => u.Login == username);
                if (user == null)
                {
                    //UserInfo newUserInfo = new UserInfo { Name = username };
                    User newUser = new User { Login = username, PasswordHash = Hashing.HashPassword(password) };
                    //newUser.Info = newUserInfo;
                    //newUserInfo.User = newUser;
                    /*ICollection<Game>? userGames = new List<Game> ();
                    userGames.Add()
                    newUser.Games = userGames;*/
                    //_db.Infos.Add(newUserInfo);
                    _db.Users.Add(newUser);
                    _db.SaveChanges();
                    await Authenticate(newUser.Login);
                    return RedirectToAction("Settings");

                }
                else
                {
                    TempData["Error"] = "Користувач існує";
                    return RedirectToAction("Signup");
                }
            }
            else
            {
                ViewBag.Error = "erorus";
                return View();
            }
        }
        public IActionResult Index()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null
                && _httpContextAccessor.HttpContext.User != null &&
                _httpContextAccessor.HttpContext.User.Identity != null &&
                !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                ViewBag.username = _httpContextAccessor.HttpContext.User.Identity.Name;
                return View();
            }
            else
            {
                return View("IndexForNew");
            }

        }
        [HttpGet]
        [Authorize]
        public IActionResult Settings()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Settings(int age = 342347653, string name = "t#45f/**sdf")
        {
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null
                && _httpContextAccessor.HttpContext.User != null &&
                _httpContextAccessor.HttpContext.User.Identity != null &&
                !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                string? username = _httpContextAccessor.HttpContext.User.Identity.Name;
                UserInfo? userInfo = await _db.Infos.FirstOrDefaultAsync<UserInfo>(i => i.User.Login == username);
                if (userInfo != null)
                {
                    if (age != 342347653)
                        userInfo.Age = age;
                    if (name != "t#45f/**sdf")
                        userInfo.Name = name;
                    _db.Infos.Update(userInfo);
                    await _db.SaveChangesAsync();
                    return View();
                }
                else
                {
                    UserInfo newUserInfo = new();
                    newUserInfo.Age = age;
                    newUserInfo.Name = name;
                    User? addingUser = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == username);
                    if (addingUser != null)
                    {
                        newUserInfo.User = addingUser;
                        newUserInfo.User.Info = newUserInfo;
                    }

                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View("IndexForNew");
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public IActionResult GetGameOptions(string gameName)
        {
            List<Game> games = _db.Games.Where(game => game.Title.ToLower().Contains(gameName.ToLower())).ToList();
            foreach(Game g in games)
            {
                g.Genre = null;
                g.Users = null;
                g.Description = null;
            }
            return Json(games);//Convert.SerializeObject(games);
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        [HttpGet]
        [Authorize]
        public IActionResult AddGenres()
        {
            string[] genres = {
                    "ACTION",
                    "ADVENTURE",
                    "ROLE_PLAYING_GAME",
                    "STRATEGY",
                    "SHOOTER",
                    "SPORTS",
                    "SIMULATION",
                    "FIGHTING",
                    "PUZZLE",
                    "PLATFORMER",
                    "RACING",
                    "MMORPG",
                    "SURVIVAL",
                    "HORROR",
                    "STEALTH",
                    "OPEN_WORLD",
                    "SANDBOX",
                    "EDUCATIONAL",
                    "MUSIC_RHYTHM",
                    "VISUAL_NOVEL"
                };

            List<Genre> genreList = new List<Genre>();

            foreach (string genreName in genres)
            {
                Genre genre = new Genre
                {
                    Name = genreName,
                    Description = GetGenreDescription(genreName),
                    Games = new List<Game>()
                };

                genreList.Add(genre);
            }
            foreach (Genre genre in genreList)
            {
                _db.Genres.Add(genre);
            }
            _db.SaveChanges();
            return RedirectToAction("AddGame");
        }
        // Метод для отримання опису жанру
        private string GetGenreDescription(string genreName)
        {
            switch (genreName)
            {
                case "ACTION":
                    return "Action games involve high levels of physical challenges, including combat, exploration, and problem-solving.";

                case "ADVENTURE":
                    return "Adventure games focus on exploration, storytelling, and puzzle-solving in a fictional or fantastical setting.";

                case "ROLE_PLAYING_GAME":
                    return "Role-playing games allow players to assume the roles of characters in a fictional world, making choices and progressing through a narrative.";

                case "STRATEGY":
                    return "Strategy games require players to plan and make strategic decisions to achieve specific goals.";

                case "SHOOTER":
                    return "Shooter games emphasize combat using ranged weapons, such as guns or projectiles.";

                case "SPORTS":
                    return "Sports games simulate various sports and allow players to participate in virtual athletic competitions.";

                case "SIMULATION":
                    return "Simulation games aim to recreate real-world activities or experiences, such as driving, flying, or managing a virtual world.";

                case "FIGHTING":
                    return "Fighting games focus on one-on-one combat between characters, often with special moves and combos.";

                case "PUZZLE":
                    return "Puzzle games require players to solve puzzles or challenges, often involving logic, patterns, or problem-solving skills.";

                case "PLATFORMER":
                    return "Platformer games feature a character navigating platforms and overcoming obstacles, often in a side-scrolling environment.";

                case "RACING":
                    return "Racing games involve competing in races, either against computer-controlled opponents or other players.";

                case "MMORPG":
                    return "MMORPGs (Massively Multiplayer Online Role-Playing Games) are online games where players interact with a large number of others in a persistent virtual world.";

                case "SURVIVAL":
                    return "Survival games challenge players to survive in a hostile environment, often with limited resources and threats.";

                case "HORROR":
                    return "Horror games aim to evoke feelings of fear and suspense through atmospheric elements and intense gameplay.";

                case "STEALTH":
                    return "Stealth games require players to avoid detection and complete objectives through stealthy and covert actions.";

                case "OPEN_WORLD":
                    return "Open-world games provide a vast and unrestricted virtual world for players to explore and interact with.";

                case "SANDBOX":
                    return "Sandbox games offer a virtual sandbox or playground where players have freedom to create, build, and experiment.";

                case "EDUCATIONAL":
                    return "Educational games are designed to teach specific skills, knowledge, or concepts in an interactive and engaging way.";

                case "MUSIC_RHYTHM":
                    return "Music rhythm games challenge players to synchronize their actions with the rhythm of music.";

                case "VISUAL_NOVEL":
                    return "Visual novels are interactive stories with primarily static visuals, often featuring branching narratives and player choices.";

                default:
                    return "No description available.";
            }
        }

    }
}