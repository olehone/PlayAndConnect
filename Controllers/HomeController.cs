using System.Timers;
using System.Xml.Schema;
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
using PlayAndConnect.ViewModels;

namespace PlayAndConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserDb _userDb;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, IUserDb userDb, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userDb = userDb;
            _webHostEnvironment = webHostEnvironment;
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
                return RedirectToAction("AddGame");
            await _db.Games.AddAsync(newGame);
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
                    if (username.Length < 28)
                    {
                        User newUser = new User { Login = username, PasswordHash = Hashing.HashPassword(password) };
                        _db.Users.Add(newUser);
                        _db.SaveChanges();
                        await Authenticate(newUser.Login);
                        return RedirectToAction("Settings");
                    }
                    else
                    {
                        TempData["Error"] = "Задовгий логін";
                        return RedirectToAction("Signup");
                    }
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
        [Authorize]
        [HttpGet]
        public IActionResult Account(string login)
        {
            if (login == null)
                return RedirectToAction("Index");
            User? findUser = _db.Users.FirstOrDefault<User>(u => u.Login == login);
            if (findUser == null)
                return RedirectToAction("Index");
            ViewBag.login = findUser.Login;
            UserInfo? findInfo = _db.Infos.FirstOrDefault<UserInfo>(i => i.User == findUser);
            if (findInfo == null)
                RedirectToAction("Index");
            ViewBag.username = findInfo.Name;
            ViewBag.age = findInfo.Age;
            ViewBag.imagePath = findInfo.ImagePath;
            ViewBag.description = findInfo.Description;
            string? currentUser = null;
            if (GetUsernameFromCookie(out currentUser))
            {
                if (currentUser != null)
                {
                    if (isMatch(currentUser, login))
                        ViewBag.contact = $"Info for match: {findInfo.Contact}";
                }
            }
            Game? game = null;
            game = _db.Games.FirstOrDefault<Game>(g => g.Users.Contains(findUser));
            if (game != null)
                ViewBag.game = game.Title;
            else
                ViewBag.description = "heh";
            return View("Account");
        }/*
[Authorize]
        [HttpGet]
        public IActionResult Chat()
        {
            return PartialView();
        }*/
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetMessages(string interlocutorLogin)
        {
            string? currentUserLogin = null;
            if (GetUsernameFromCookie(out currentUserLogin))
            {
                Console.WriteLine("!");
                User? currentUser = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == currentUserLogin);
                User? interlocutor = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == interlocutorLogin);
                Console.WriteLine("All right");
                if (currentUser != null && interlocutor != null)
                {
                    Console.WriteLine("!124212");
                    Console.WriteLine(currentUser.Login);
                    Console.WriteLine(interlocutor.Login);

                    //Chat? chat = _db.Chats.FirstOrDefault(c => c.Id==1);
                    Chat? chat = await _db.Chats.FirstOrDefaultAsync<Chat>(c => c.Users.Any(u => u.Id == currentUser.Id) && c.Users.Any(u => u.Id == interlocutor.Id));
                    //Chat? chat = _db.Chats.FirstOrDefault(c => c.Users.Contains(currentUser) && c.Users.Contains(interlocutor));
                    if (chat != null)
                    {
                        Console.WriteLine("!124124124124");
                        List<MessageViewModel> messagesForView = new();
                        List<Message> messagesFromDb = await _db.Messages.Where<Message>(m => m.Chat == chat).ToListAsync();
                        foreach (Message m in messagesFromDb)
                        {
                            MessageViewModel mForView = new();
                            if (m.User == null)
                                mForView.IsSystem = true;
                            else
                                mForView.IsSystem = false;
                            if (m.User == currentUser)
                                mForView.IsOwn = true;
                            else
                                mForView.IsOwn = false;
                            if (m.Text != null)
                                mForView.Text = m.Text;
                            else mForView.Text = "Не вийшло отримати повідомлення";
                            mForView.Time = m.TimeOfSending.ToString("dd MMMM HH:mm");
                            messagesForView.Add(mForView);
                            Console.WriteLine("!1134134613");
                        }
                        Console.WriteLine("!443141");
                        return Json(messagesForView);
                    }
                    else
                    {
                        Console.WriteLine("All so geioghaljkdbad");
                        return Ok();
                    }

                }
                Console.WriteLine("All rightasdsadgasgdag");
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login");
        }
        [Authorize]
        [HttpGet]
        public IActionResult Chats()
        {
            string? currentUserLogin = null;
            if (GetUsernameFromCookie(out currentUserLogin))
            {
                User? user = _db.Users.FirstOrDefault<User>(u => u.Login == currentUserLogin);
                if (user != null)
                {
                    List<Chat>? chats = new();
                    chats = _db.Chats.Include(c => c.Users)
                                    .ThenInclude(u => u.Info)
                                    .Where(c => c.Users.Contains(user))
                                    .ToList();
                    if (chats != null)
                    {
                        Console.WriteLine("Chats not null");
                        List<ChatViewModel>? userChats = new List<ChatViewModel>();
                        foreach (Chat c in chats)
                        {
                            Console.WriteLine("I make a chaaat");
                            ChatViewModel? userChat = new();
                            User? notCurrentUser = null;
                            notCurrentUser = c.Users.FirstOrDefault<User>(u => u.Id != user.Id);
                            if (notCurrentUser != null)
                            {
                                Console.WriteLine("Chats or noooooooooooooooooooo");
                                Message? message = GetLastMessage(c);
                                if (message != null)
                                {
                                    if (message.Text != null)
                                        userChat.Message = message.Text;
                                    else
                                        userChat.Message = "Cant read message";
                                    userChat.TimeOfLastMessage = message.TimeOfSending;
                                    if (message.Id == c.Messages.Max(m => m.Id))
                                        userChat.IsNew = true;
                                }
                                Console.WriteLine(userChat.Message);
                                userChat.Login = notCurrentUser.Login;
                                Console.WriteLine(userChat.Login);
                                UserInfo? info = _db.Infos.FirstOrDefault<UserInfo>(i => i.User == notCurrentUser);
                                Console.WriteLine(info.Id);
                                userChat.ImagePath = info.ImagePath;
                                Console.WriteLine(userChat.ImagePath);
                                userChat.IsSelected = false;
                                userChats.Add(userChat);
                            }
                            else
                                Console.WriteLine("Baaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaad");

                        }
                        /*
                        ViewBag.Chats = chats.Select(c => new
                        {
                            User = c.Users.Where(u => u.Login != currentUserLogin)
                            .Select(u => new { Login = u.Login, ImagePath = u.Info.Name }),
                            LastMessageText = GetLastMessage(c)
                        });*/
                        if (userChats.Any())
                        {
                            Console.WriteLine(userChats.FirstOrDefault().Message);
                            List<ChatViewModel> sortedChats = userChats.OrderBy(chat => chat.TimeOfLastMessage).Reverse().ToList();
                            //sortedChats.Remove(sortedChats.LastOrDefault(c=> c.Message!="salgjas"));
                            ViewBag.Chats = sortedChats;
                            foreach (ChatViewModel chat in userChats)
                            {
                                Console.WriteLine(chat.Message);
                                Console.WriteLine(chat.Login);
                                Console.WriteLine(chat.ImagePath);
                                Console.WriteLine("LHafhs");

                            }
                            Console.WriteLine("SORTED");
                            foreach (ChatViewModel chat in sortedChats)
                            {
                                Console.WriteLine(chat.Message);
                                Console.WriteLine(chat.Login);
                                Console.WriteLine(chat.ImagePath);
                                Console.WriteLine("LHafhs");

                            }
                            return View();
                        }
                        ViewBag.SysMessage = " No chats. Go to mathes!";
                        return View();
                    }
                    else
                    {
                        ViewBag.SysMessage = "Error: No chats";
                        return RedirectToAction("Match");
                    }
                }
                else
                {
                    TempData["SysMessage"] = "Error: No matches";
                    return View();
                }
            }
            else
            {
                TempData["SysMessage"] = "Error: Non in system. Login, please";
                return RedirectToAction("Login");
            }
        }
        [Authorize]
        [HttpPost]
        public bool SendMessage(string message, string login)
        {
            string? currentUserLogin = null;

            if (GetUsernameFromCookie(out currentUserLogin))
            {
                User? currentUser = _db.Users.FirstOrDefault<User>(u => u.Login == currentUserLogin);
                if (currentUser != null)
                {
                    Chat? chat = _db.Chats.FirstOrDefault(c => c.Users.Any(u => u.Login == login) && c.Users.Any(u => u.Login == currentUserLogin));
                    if (chat != null)
                    {
                        Console.WriteLine("lsdglhasldgh");
                        Message? messageForChat = new();
                        messageForChat.Text = message;
                        messageForChat.TimeOfSending = DateTime.Now;
                        messageForChat.User = currentUser;
                        messageForChat.Chat = chat;
                        if (chat.Messages != null)
                            chat.Messages.Add(messageForChat);
                        else
                        {
                            chat.Messages = new List<Message>();
                            chat.Messages.Add(messageForChat);
                        }
                        _db.Chats.Update(chat);
                        _db.Messages.Add(messageForChat);
                        _db.SaveChanges();
                        return true;
                    }
                    else return false;
                }
                else return false;

                /*
                User? currentUser = _db.Users.FirstOrDefault<User>(u => u.Login == currentUserLogin);
                if (currentUser != null)
                {
                    User? otherUser = _db.Users.FirstOrDefault<User>(u => u.Login == login);
                    if (otherUser != null)
                    {
                        Chat? currentChat = null;

                    }
                    else
                        return false;
                }
                else
                    return false;*/
            }
            else return false;
        }
        [Authorize]
        [HttpGet]
        public PartialViewResult? currentAccount(string login)
        {
            if (login == null)
                return null;//PartialView();
            User? findUser = _db.Users.FirstOrDefault<User>(u => u.Login == login);
            if (findUser == null)
                return null;//RedirectToAction("Index");
            ViewBag.login = findUser.Login;
            UserInfo? findInfo = _db.Infos.FirstOrDefault<UserInfo>(i => i.User == findUser);
            if (findInfo == null)
                return null;//RedirectToAction("Index");
            ViewBag.username = findInfo.Name;
            ViewBag.age = findInfo.Age;
            ViewBag.imagePath = findInfo.ImagePath;
            ViewBag.description = findInfo.Description;
            string? currentUser = null;
            if (GetUsernameFromCookie(out currentUser))
            {
                if (currentUser != null)
                {
                    if (isMatch(currentUser, login))
                        ViewBag.contact = $"Info for match: {findInfo.Contact}";
                }
            }
            Game? game = null;
            game = _db.Games.FirstOrDefault<Game>(g => g.Users.Contains(findUser));
            if (game != null)
                ViewBag.game = game.Title;
            else
                ViewBag.description = "heh";
            return PartialView("Account");
        }
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public IActionResult Accounts(bool like)
        {
            return RedirectToAction("Index");
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
        private bool CreateNewChat(User user1, User user2)
        {
            Console.WriteLine("Newwwww chaaaaat");
            Console.WriteLine(user1.Login);
            Console.WriteLine(user2.Login);
            Chat? chat = _db.Chats.FirstOrDefault(c => c.Users.Any(u => u.Id == user1.Id) && c.Users.Any(u => u.Id == user2.Id));

            //Chat? chat = _db.Chats.FirstOrDefault<Chat>(c => c.Users.Contains(user1) && c.Users.Contains(user2));
            if (chat == null)
            {
                Chat newChat = new();
                newChat.Messages = new List<Message>();
                newChat.Users = new List<User>();
                newChat.Users.Add(user1);
                newChat.Users.Add(user2);
                Console.WriteLine("Add users");
                Message message = new();//CreatingMessage(newChat, null, "Взаємний інтерес! Чат створено");
                message.Chat = newChat;
                message.Text = "Взаємний інтерес! Чат створено";
                message.User = null;
                message.TimeOfSending = DateTime.Now;
                newChat.Messages.Add(message);
                _db.Add<Message>(message);
                _db.Add<Chat>(newChat);
                _db.SaveChanges();
                Console.WriteLine("SaveChanges");
                return true;
            }
            else return false;
        }
        [NonAction]
        private bool addSystemMessage(Chat chat, string text)
        {
            //Chat? chat = _db.Chats.FirstOrDefault<Chat>(c => c.Users.Contains(user1) && c.Users.Contains(user2));
            if (chat != null)
            {
                Message message = new CreatingMessage(chat, null, text);
                _db.Add<Message>(message);
                _db.Update<Chat>(chat);
                _db.SaveChanges();
                return true;
            }
            else return false;
        }
        [NonAction]
        private bool UpDateLike(User user1, User user2, bool firstLikeSecond)
        {
            Console.WriteLine("1132513");
            Like? like = _db.Likes.FirstOrDefault<Like>(l => l.User1Id == user1.Id && l.User2Id == user2.Id);
            if (like != null)
            {
                Console.WriteLine("2");

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
                Console.WriteLine("3253325");

                _db.Likes.Update(like);
                _db.SaveChanges();
                if (like.User1LikesUser2 && like.User2LikesUser1)
                {
                    Console.WriteLine("Must create a new chat!");
                    CreateNewChat(user1, user2);
                }
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
                    if (like.User1LikesUser2 && like.User2LikesUser1)
                    {
                        Console.WriteLine("Must create a new chat!");
                        CreateNewChat(user1, user2);
                    }
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
            string? username;
            UserInfo? info = null;
            if (GetUsernameFromCookie(out username))
            {
                info = _db.Infos.FirstOrDefault<UserInfo>(i => i.User.Login == username);
            }
            if (info != null)
            {
                @ViewBag.Name = info.Name;
                @ViewBag.Age = info.Age;
                @ViewBag.Description = info.Description;
                @ViewBag.Contact = info.Contact;
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Settings(string name, int age, IFormFile imageFile, int selectGame = 1000000, string description = "No description", string contact = "No contact information")
        {
            string? username;
            if (GetUsernameFromCookie(out username))
            {
                User? user = await _db.Users.FirstOrDefaultAsync<User>(u => u.Login == username);
                bool isUserModif = false, isInfoModif = false;
                UserInfo? userInfo = null;
                Game? game = null;
                if (user != null)
                    userInfo = user.Info;
                if (selectGame != 1000000)
                    game = await _db.Games.FirstOrDefaultAsync<Game>(g => g.Id == selectGame);
                else
                {
                    game = await _db.Games.FirstOrDefaultAsync<Game>(g => g.Id == 1);
                }
                if (user != null && game != null && age > 7 && age < 100 && name != null)
                {
                    if (userInfo != null)
                    {
                        userInfo.Age = age;
                        userInfo.Name = name;
                        userInfo.Description = description;
                        userInfo.Contact = contact;
                        isInfoModif = true;
                        ICollection<Game>? games = user.Games;
                        if (games == null)
                        {
                            games = new List<Game>();
                            user.Games = games;
                            games.Add(game);
                            isUserModif = true;
                        }
                        else
                        {
                            if (!(games.Contains(game)))
                            {
                                games.Add(game);
                                isUserModif = true;
                                Console.WriteLine("Dont contains");
                            }
                            else
                            {
                                Console.WriteLine("Contains all must be right");
                                Console.WriteLine(games.FirstOrDefault<Game>().Title);
                            }

                        }
                        if (imageFile != null && imageFile.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                            string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);
                            using (var stream = new FileStream(imagePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(stream);
                            }
                            userInfo.ImagePath = "/images/" + uniqueFileName; // Збереження шляху до зображення
                            isInfoModif = true;
                        }

                        if (isInfoModif)
                            _db.Infos.Update(userInfo);
                        if (isUserModif)
                        {
                            Console.WriteLine("Update user");
                            _db.Users.Update(user);
                        }
                        await _db.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        UserInfo newUserInfo = new();
                        newUserInfo.Age = age;
                        newUserInfo.Name = name;
                        newUserInfo.Description = description;
                        newUserInfo.Contact = contact;
                        ICollection<Game>? games = user.Games;
                        if (games == null)
                        {
                            games = new List<Game>();
                            user.Games = games;
                        }
                        games.Add(game); // Додати гру до списку ігор

                        if (imageFile != null && imageFile.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                            string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);
                            using (var stream = new FileStream(imagePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(stream);
                            }
                            newUserInfo.ImagePath = "/images/" + uniqueFileName; // Збереження шляху до зображення
                        }
                        else
                        {
                            newUserInfo.ImagePath = "/images/default.jpg";
                        }

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
                            Console.WriteLine("May be problem1");
                            likes = _db.Likes.Where(l => l.User2Id == user.Id).ToList();
                            userId = likes.FirstOrDefault(l => l.User2LikesUser1 == true && l.User1LikesUser2 == false)?.User1Id;
                        }
                        if (userId != null)
                        {
                            Console.WriteLine("May be problem2");
                            return await _db.Users.FirstOrDefaultAsync<User>(u => u.Id == userId);
                        }
                    }
                    ICollection<Game> games = user.Games;
                    Console.WriteLine("May be problem3");
                    if (games != null)
                    {
                        ICollection<Genre> genres = new List<Genre>();
                        Console.WriteLine("May be problem4");
                        foreach (Game g in games)
                        {
                            Console.WriteLine("May be problem5");
                            if (!genres.Contains(g.Genre))
                                genres.Add(g.Genre);
                        }
                        ICollection<Game> gamesFromGenre = new List<Game>();
                        ICollection<Game> gamesMatch = new List<Game>();
                        ICollection<User> usersMatch = new List<User>();
                        ICollection<User>? usersFromGames = new List<User>();
                        if (genres != null)
                        {
                            Console.WriteLine("May be problem6");
                            foreach (Genre g in genres)
                            {
                                Console.WriteLine("May be problem7");
                                if (g != null)
                                {
                                    gamesFromGenre = g.Games;
                                    foreach (Game gem in gamesFromGenre)
                                    {
                                        Console.WriteLine("May be problem8");
                                        if (!gamesMatch.Contains(gem))
                                        {
                                            Console.WriteLine("May be problem9");
                                            gamesMatch.Add(gem);
                                        }
                                    }
                                }
                                else
                                {

                                    Console.WriteLine("May be problem23r2e6");
                                    return null;
                                }
                            }
                            foreach (Game ge in gamesMatch)
                            {
                                Console.WriteLine("May be problem12");
                                usersFromGames = ge.Users;
                                if (usersFromGames != null)
                                {
                                    Console.WriteLine("May be problem122");
                                    foreach (User u in usersFromGames)
                                    {
                                        if (!usersMatch.Contains(u))
                                        {
                                            Console.WriteLine("May be problem235135");
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
        private bool isMatch(string loginWhoSearches, string loginWhoChecked)
        {
            User? userWhoSearches = _db.Users.FirstOrDefault<User>(u => u.Login == loginWhoSearches);
            if (userWhoSearches == null)
                return false;
            User? userWhoChecked = _db.Users.FirstOrDefault<User>(u => u.Login == loginWhoChecked);
            if (userWhoChecked == null)
                return false;
            Like? like = _db.Likes.FirstOrDefault<Like>(l => l.User1Id == userWhoSearches.Id && l.User2Id == userWhoChecked.Id);
            if (like != null)
            {
                if (like.User1LikesUser2 && like.User2LikesUser1)
                    return true;
            }
            like = _db.Likes.FirstOrDefault<Like>(l => l.User2Id == userWhoSearches.Id && l.User1Id == userWhoChecked.Id);
            if (like != null)
            {
                if (like.User1LikesUser2 && like.User2LikesUser1)
                    return true;
            }
            return false;
        }
        [NonAction]
        private bool isUserNonInUnlike(User userWhoSearches, User userWhoChecked)
        {
            Like? like = _db.Likes.FirstOrDefault<Like>(l => l.User1Id == userWhoSearches.Id && l.User2Id == userWhoChecked.Id);
            if (like != null)
            {
                if ((!like.User1LikesUser2 && !like.User2LikesUser1)
                || (like.User1LikesUser2 && !like.User2LikesUser1)
                || (like.User1LikesUser2 && like.User2LikesUser1))
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
                || (like.User2LikesUser1 && !like.User1LikesUser2)
                || (like.User1LikesUser2 && like.User2LikesUser1))
                    return false;
                else return true;
            }
            else return true;
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
        [NonAction]
        private Message? GetLastMessage(Chat chat)
        {
            Message? message = null;

            if (chat.Messages != null && chat.Messages.Any())
            {
                //message = chat.Messages.FirstOrDefault<Message>(m => m.Id == chat.Messages.Max<Message>(m => m.Id));
                message = chat.Messages.LastOrDefault();
            }
            return message;
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
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(15) // Додаємо 15 днів до поточної дати
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), authProperties);
        }
        /*
        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }*/
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