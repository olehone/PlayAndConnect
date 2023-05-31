using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
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
                    User newUser = new User { Login = username, PasswordHash = Hashing.HashPassword(password) };
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
        public async Task<IActionResult> Match()
        {
            Console.WriteLine("Getting user");
            User? userForView = await GetUserForMatch();
            if (userForView != null)
            {
                ViewBag.Username = userForView.Info.Name;
                ViewBag.Login = userForView.Login;
                ViewBag.Age = userForView.Info.Age;
                string login = userForView.Login;
                TempData["LoginMatch"] = login;
                return View();
            }
            else
            {
                TempData["Error"] = "No user in match";
                return RedirectToAction("Index");
            }
        }
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public IActionResult Match(bool like)
        {
            if (TempData.ContainsKey("LoginMatch"))
            {
                string? login = (string)TempData["LoginMatch"];
                User? getUserFromMatch;
                if (login != null)
                {
                    getUserFromMatch = _db.Users.FirstOrDefault<User>(u => u.Login == login);
                }
                else
                {
                    return RedirectToAction("Index");
                }
                TempData.Remove("LoginMatch");
                if (getUserFromMatch == null)
                    return RedirectToAction("Index");
                string? currentUsername;
                if (!(GetUsernameFromCookie(out currentUsername)))
                {
                    TempData["Error"] = "Не вийшло дістати користувача з кукі";
                    return RedirectToAction("Index");
                }
                User? currentUser = _db.Users.FirstOrDefault<User>(u => u.Login == currentUsername);
                if (currentUser == null)
                    return RedirectToAction("Index");
                if (!UpDateLike(currentUser, getUserFromMatch, like))
                {
                    TempData["Error"] = "Не вийшло оновити лайк";
                    return RedirectToAction("Index");
                }
                else
                    return RedirectToAction("Match");
            }
            else
            {
                return RedirectToAction("Match");
            }

        }
        [NonAction]
        private bool UpDateLike(User user1, User user2, bool firstLikeSecond)
        {
            Like? like = _db.Likes.FirstOrDefault<Like>(l => l.User1Id == user1.Id && l.User2Id == user2.Id);
            if (like != null)
            {
                if (!like.User1LikesUser2 && !like.User2LikesUser1)
                    return false;
                if (!firstLikeSecond)
                {
                    like.User1LikesUser2 = false;
                    like.User2LikesUser1 = false;
                    _db.Likes.Update(like);
                    _db.SaveChanges();
                    return true;
                }
                like.User1LikesUser2 = firstLikeSecond;
                _db.Likes.Update(like);
                _db.SaveChanges();
                return true;
            }
            else
            {

                like = _db.Likes.FirstOrDefault<Like>(l => l.User2Id == user1.Id && l.User1Id == user2.Id);

                if (like != null)
                {
                    if (!like.User1LikesUser2 && !like.User2LikesUser1)
                        return false;
                    if (!firstLikeSecond)
                    {
                        like.User1LikesUser2 = false;
                        like.User2LikesUser1 = false;
                        _db.Likes.Update(like);
                        _db.SaveChanges();
                        return true;
                    }
                    like.User2LikesUser1 = firstLikeSecond;
                    _db.Likes.Update(like);
                    _db.SaveChanges();
                    return true;
                }
                else
                {

                    like = new();
                    like.User1Id = user1.Id;
                    like.User2Id = user2.Id;
                    if (firstLikeSecond)
                    {
                        like.User1LikesUser2 = firstLikeSecond;
                    }
                    else
                    {
                        like.User1LikesUser2 = false;
                        like.User2LikesUser1 = false;
                    }
                    _db.Likes.Add(like);
                    _db.SaveChanges();
                    return true;
                }
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
        public async Task<IActionResult> Settings(string name, int selectGame, int age)
        {
            string? username;
            if (GetUsernameFromCookie(out username))
            {
                User? user = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == username);
                UserInfo? userInfo = null;
                if (user != null)
                    userInfo = user.Info;
                Game? game = await _db.Games.FirstOrDefaultAsync<Game>(g => g.Id == selectGame);
                if (user != null && game != null && age > 7 && age < 100 && name != null)
                {
                    if (userInfo != null)
                    {
                        userInfo.Age = age;
                        userInfo.Name = name;
                        ICollection<Game>? games = user.Games;
                        if (games == null)
                        {
                            games = new List<Game>();
                            user.Games = games;
                        }
                        games.Add(game); // Додати гру до списку ігор
                        _db.Infos.Update(userInfo);
                        _db.Users.Update(user);
                        await _db.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        UserInfo newUserInfo = new();
                        newUserInfo.Age = age;
                        newUserInfo.Name = name;
                        ICollection<Game>? games = user.Games;
                        if (games == null)
                        {
                            games = new List<Game>();
                            user.Games = games;
                        }
                        games.Add(game); // Додати гру до списку ігор
                        User? addingUser = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == username);
                        if (addingUser != null)
                        {
                            newUserInfo.User = addingUser;
                            newUserInfo.User.Info = newUserInfo;
                        }
                        _db.Users.Update(user);
                        await _db.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("Setting");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        /*
        [NonAction]
        private User? GetUserForMetch()
        {
            string? username;
            if (GetUsernameFromCookie(out username))
            {
                User? user = _db.Users.FirstOrDefault<User>(u => u.Login == username);
                if (user != null)
                {
                    UserInfo? info = user.Info;
                    IEnumerable<Like>? likes = user.Likes;
                    if (likes?.Any() == true)
                    {
                        int? userId = likes.FirstOrDefault(l => l.User1Id == user.Id && !l.User1LikesUser2 && l.User2LikesUser1)?.User2Id;
                        if (userId == null)
                        {
                            userId = likes.FirstOrDefault(l => l.User2Id == user.Id && !l.User2LikesUser1 && l.User1LikesUser2)?.User2Id;
                        }
                        if (userId != null)
                        {
                            return _db.Users.FirstOrDefault<User>(u => u.Id == userId);
                        }
                    }
                    ICollection<Game>? games = user.Games;
                    if (games?.Any() == true)
                    {
                        ICollection<Genre> genres = new List<Genre>();
                        foreach (Game g in games)
                        {
                            if (!genres.Contains(g.Genre))
                                genres.Add(g.Genre);
                        }
                        ICollection<Game> gamesFromGenre = new List<Game>();
                        ICollection<Game> gamesMatch = new List<Game>();
                        ICollection<User> usersMatch = new List<User>();
                        ICollection<User>? usersFromGames = new List<User>();
                        if (genres.Any())
                        {
                            foreach (Genre g in genres)
                            {
                                gamesFromGenre = g.Games;
                                foreach (Game gem in gamesFromGenre)
                                {
                                    if (!gamesMatch.Contains(gem))
                                    {
                                        gamesMatch.Add(gem);
                                    }
                                }
                            }
                            foreach (Game ge in gamesMatch)
                            {
                                usersFromGames = ge.Users;
                                if (usersFromGames?.Any() == true)
                                {
                                    foreach (User u in usersFromGames)
                                    {
                                        if (!usersMatch.Contains(u))
                                        {
                                            usersMatch.Add(u);
                                        }
                                    }
                                    usersMatch.Remove(user);
                                }
                            }
                            if (usersMatch.Any())
                            {
                                foreach (User returnUser in usersMatch)
                                {
                                    if (isUserNonInUnlike(user, returnUser))
                                    {
                                        return returnUser;
                                    }
                                }
                                return null;
                            }
                        }
                    }
                    else
                    {
                        TempData["Error"] = "No games";
                        return null;
                    }
                }
            }
            return null;
        }
        */
        [NonAction]
        private async Task<User?> GetUserForMatch()
        {
            string? username;
            if (GetUsernameFromCookie(out username))
            {
                User? user = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == username);
                if (user != null)
                {
                    UserInfo? info = user.Info;
                    Console.WriteLine("May be problem");
                    IEnumerable<Like>? likes = _db.Likes.Where(l => l.User1Id == user.Id).ToList();
                    if (likes != null)
                    {
                        int? userId = likes.FirstOrDefault(l => l.User1LikesUser2 == false && l.User2LikesUser1 == true)?.User2Id;
                        if (userId == null)
                        {
                            likes = _db.Likes.Where(l => l.User2Id == user.Id).ToList();
                            userId = likes.FirstOrDefault(l => l.User2LikesUser1 == true && l.User1LikesUser2 == false)?.User1Id;
                        }
                        if (userId != null)
                        {
                            return await _db.Users.FirstOrDefaultAsync<User>(u => u.Id == userId);
                        }
                    }
                    ICollection<Game> games = user.Games;
                    if (games != null)
                    {
                        ICollection<Genre> genres = new List<Genre>();
                        foreach (Game g in games)
                        {
                            if (!genres.Contains(g.Genre))
                                genres.Add(g.Genre);
                        }
                        ICollection<Game> gamesFromGenre = new List<Game>();
                        ICollection<Game> gamesMatch = new List<Game>();
                        ICollection<User> usersMatch = new List<User>();
                        ICollection<User>? usersFromGames = new List<User>();
                        if (genres != null)
                        {
                            foreach (Genre g in genres)
                            {
                                gamesFromGenre = g.Games;
                                foreach (Game gem in gamesFromGenre)
                                {
                                    if (!gamesMatch.Contains(gem))
                                    {
                                        gamesMatch.Add(gem);
                                    }
                                }
                            }
                            foreach (Game ge in gamesMatch)
                            {
                                usersFromGames = ge.Users;
                                if (usersFromGames != null)
                                {
                                    foreach (User u in usersFromGames)
                                    {
                                        if (!usersMatch.Contains(u))
                                        {
                                            usersMatch.Add(u);
                                        }
                                    }
                                    usersMatch.Remove(user);
                                }
                            }
                            if (usersMatch == null)
                                return null;
                            else
                            {
                                foreach (User returnUser in usersMatch)
                                {
                                    if (isUserNonInUnlike(user, returnUser))
                                    {
                                        return returnUser;
                                    }
                                }
                                return null;
                            }
                        }
                        else
                            return null;

                    }
                    else
                    {
                        TempData["Error"] = "No games";
                        RedirectToAction("Settings");
                        return null;
                    }
                }
                else
                    return null;
            }
            else
                return null;
        }

        [NonAction]
        private bool isUserNonInUnlike(User userWhoSearches, User userWhoChecked)
        {
            Like? like = _db.Likes.FirstOrDefault<Like>(l => l.User1Id == userWhoSearches.Id && l.User2Id == userWhoChecked.Id);
            if(like!= null)
            {
                if ((!like.User1LikesUser2 && !like.User2LikesUser1)
                ||(like.User1LikesUser2&&!like.User2LikesUser1)
                ||(like.User1LikesUser2&&like.User2LikesUser1))
                    return false;
                else return true;
            }
            else
            {
                like = _db.Likes.FirstOrDefault<Like>(l => l.User2Id == userWhoSearches.Id && l.User1Id == userWhoChecked.Id);
            }
            if (like != null)
            {
                if ((!like.User2LikesUser1 && !like.User1LikesUser2)
                ||(like.User2LikesUser1&&!like.User1LikesUser2)
                ||(like.User1LikesUser2&&like.User2LikesUser1))
                    return false;
                else return true;
            }
            else return true;
            /*
            List<Like> chekedLikes = (List<Like>)userWhoChecked.Likes;
            if (chekedLikes == null)
                return true;
            int checkId = userWhoChecked.Id;
            foreach (Like checkLike in chekedLikes)
            {
                if (checkLike.User1Id == checkId || checkLike.User2Id == checkId)
                {
                    if (!checkLike.User1LikesUser2 && !checkLike.User2LikesUser1)
                    {
                        return false;
                    }
                }
            }
            return true;
            */

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
            return Json(games.Select(g => new { Id = g.Id, Title = g.Title }));
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
        [NonAction]
        private bool GetUsernameFromCookie(out string? username)
        {
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null
                && _httpContextAccessor.HttpContext.User != null &&
                _httpContextAccessor.HttpContext.User.Identity != null &&
                !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.User.Identity.Name))
            {
                username = _httpContextAccessor.HttpContext.User.Identity.Name;
                return true;
            }
            else
            {
                username = null;
                return false;
            }
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