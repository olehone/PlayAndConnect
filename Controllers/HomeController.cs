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
            _userDb = userDb;
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddGame()
        {
            if (TempData.ContainsKey("Error"))
            {
                ViewBag.Error = TempData["Error"];
                TempData.Remove("Error");
            }
            Genre? cheak = await _db.Genres.FirstOrDefaultAsync<Genre>(g => g.Id == 3);
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
            Genre? genre = await _db.Genres.FirstOrDefaultAsync<Genre>(g => g.Id == selectGenre);
            newGame.Description = description;
            newGame.Title = title;
            if (genre != null)
                newGame.Genre = genre;
            else
                return RedirectToAction("AddGame");
            await _db.Games.AddAsync(newGame);
            await _db.SaveChangesAsync();
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
        public async Task<IActionResult> Login(string login, string password)
        {
            await Logout();
            if (ModelState.IsValid)
            {
                User? user = await _userDb.GetUserByPassword(login, password);
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
        public async Task<IActionResult> Signup(string login, string password, string confirm_password)
        {
            await Logout();
            if (ModelState.IsValid)
            {
                if (password != confirm_password)
                {
                    TempData["username"] = login;
                    TempData["Error"] = "Паролі не співпадають";
                    return RedirectToAction("Signup");
                }
                User? user = await _userDb.GetUserByLogin(login);
                if (user == null)
                {
                    if (login.Length < 28)
                    {
                        User? newUser = await _userDb.Create(login, password);
                        if (newUser != null)
                        {
                            await Authenticate(newUser.Login);
                            return RedirectToAction("Settings");
                        }
                        else
                            return RedirectToAction("Signup");
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
                UserInfo? infoAboutUser = await _db.Infos.FirstOrDefaultAsync<UserInfo>(i => i.UserId == userForView.Id);
                ViewBag.Login = userForView.Login;
                ViewBag.Age = userForView.Info.Age;
                string login = userForView.Login;
                if (infoAboutUser != null)
                {
                    ViewBag.Username = infoAboutUser.Name;
                }
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
        public async Task<IActionResult> Account(string login)
        {
            if (login == null)
                return RedirectToAction("Index");
            User? findUser = await _userDb.GetUserByLogin(login);
            if (findUser == null)
                return RedirectToAction("Index");
            ViewBag.login = findUser.Login;
            UserInfo? findInfo = await _db.Infos.FirstOrDefaultAsync<UserInfo>(i => i.User == findUser);
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
                    if (await isMatch(currentUser, login))
                        ViewBag.contact = $"Info for match: {findInfo.Contact}";
                }
            }
            Game? game = null;
            game = await _db.Games.FirstOrDefaultAsync<Game>(g => g.Users.Contains(findUser));
            if (game != null)
                ViewBag.game = game.Title;
            else
                ViewBag.description = "heh";
            return View("Account");
        }/*
[Authorize]
        [HttpGet]
        public Task<IActionResult> Chat()
        {
            return PartialView();
        }*/
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetMessages(string interlocutorLogin, string lastMessageText)
        {
            string? currentUserLogin = null;
            if (GetUsernameFromCookie(out currentUserLogin))
            {
                if (currentUserLogin != null)
                {
                    User? currentUser = await _userDb.GetUserByLogin(currentUserLogin);
                    User? interlocutor = await _userDb.GetUserByLogin(interlocutorLogin);
                    Console.WriteLine("All right");
                    if (currentUser != null && interlocutor != null)
                    {
                        Console.WriteLine("!124212");
                        Console.WriteLine(currentUser.Login);
                        Console.WriteLine(interlocutor.Login);

                        //Chat? chat = await _db.Chats.FirstOrDefaultAsync(c => c.Id==1);
                        Chat? chat = await _db.Chats.FirstOrDefaultAsync<Chat>(c => c.Users.Any(u => u.Id == currentUser.Id) && c.Users.Any(u => u.Id == interlocutor.Id));
                        //Chat? chat = await _db.Chats.FirstOrDefaultAsync(c => c.Users.Contains(currentUser) && c.Users.Contains(interlocutor));
                        if (chat != null)
                        {
                            Console.WriteLine(lastMessageText);
                            Console.WriteLine("Тут");
                            List<MessageViewModel> messagesForView = new();
                            List<Message> messagesFromDb = await _db.Messages.Where<Message>(m => m.Chat == chat).ToListAsync();
                            Message? checkMessage = messagesFromDb.LastOrDefault<Message>();
                            if (checkMessage.Text == lastMessageText)
                            {
                                return Json(false);
                            }
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
                }
                Console.WriteLine("All rightasdsadgasgdag");
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Chats()
        {
            string? currentUserLogin = null;
            if (GetUsernameFromCookie(out currentUserLogin))
            {
                User? user = await _userDb.GetUserByLogin(currentUserLogin);
                if (user != null)
                {
                    List<Chat>? chats = new();
                    chats = await _db.Chats.Include(c => c.Users)
                                    .ThenInclude(u => u.Info)
                                    .Where(c => c.Users.Contains(user))
                                    .ToListAsync();
                    if (chats.Any())
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
                                if (c != null)
                                {
                                    Console.WriteLine("Chat is on null");
                                    if (c.Messages != null)
                                    {
                                        if (c.Messages.Any())
                                        {
                                            Message? m = _db.Messages.FirstOrDefault<Message>(m => m.Chat.Id == c.Id);
                                            if (m != null)
                                                Console.WriteLine("lol");
                                            else
                                                Console.WriteLine("kek");
                                        }
                                    }
                                    else
                                        Console.WriteLine("Повідомленння так собі");
                                }
                                else
                                    Console.WriteLine("chat is null");
                                Message? message = await GetLastMessage(c);
                                Console.WriteLine("GetLastMessage");
                                if (message != null)
                                {
                                    Console.WriteLine(message.Text);
                                    Console.WriteLine(message.TimeOfSending.ToString("HH:mm MMMM dd"));
                                }
                                else
                                    Console.WriteLine("I dont write this");
                                if (message != null)
                                {
                                    if (message.Text != null)
                                    {
                                        if (message.Text.Count() < 33)
                                            userChat.Message = message.Text;
                                        else
                                            userChat.Message = message.Text.Substring(0, 28) + "..";
                                    }
                                    else
                                        userChat.Message = "Cant read message";
                                    userChat.TimeOfLastMessage = message.TimeOfSending;
                                    if (message.Id == c.Messages.Max(m => m.Id))
                                        userChat.IsNew = true;
                                }
                                Console.WriteLine(userChat.Message);
                                userChat.Login = notCurrentUser.Login;
                                Console.WriteLine(userChat.Login);
                                UserInfo? info = await _db.Infos.FirstOrDefaultAsync<UserInfo>(i => i.User == notCurrentUser);
                                Console.WriteLine(info.Id);
                                userChat.ImagePath = info.ImagePath;
                                Console.WriteLine(userChat.ImagePath);
                                if (userChat.IsSelected)
                                    Console.WriteLine("nosele");
                                else
                                    Console.WriteLine("yesele");
                                if (userChat.IsSelected == null)
                                    Console.WriteLine("nonosele");
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
        public async Task<bool> SendMessage(string message, string login)
        {
            string? currentUserLogin = null;

            if (GetUsernameFromCookie(out currentUserLogin))
            {
                User? currentUser = await _userDb.GetUserByLogin(currentUserLogin);
                if (currentUser != null)
                {
                    Chat? chat = await _db.Chats.FirstOrDefaultAsync(c => c.Users.Any(u => u.Login == login) && c.Users.Any(u => u.Login == currentUserLogin));
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
                        await _db.Messages.AddAsync(messageForChat);
                        await _db.SaveChangesAsync();
                        return true;
                    }
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult?> currentAccount(string login)
        {
            if (login == null)
                return null;//PartialView();
            User? findUser = await _userDb.GetUserByLogin(login);
            if (findUser == null)
                return null;//RedirectToAction("Index");
            ViewBag.login = findUser.Login;
            UserInfo? findInfo = await _db.Infos.FirstOrDefaultAsync<UserInfo>(i => i.User == findUser);
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
                    if (await isMatch(currentUser, login))
                        ViewBag.contact = $"Info for match: {findInfo.Contact}";
                }
            }
            Game? game = null;
            game = await _db.Games.FirstOrDefaultAsync<Game>(g => g.Users.Contains(findUser));
            if (game != null)
                ViewBag.game = game.Title;
            else
                ViewBag.description = "heh";
            return PartialView("Account");
        }
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public IActionResult Accounts()
        {
            return RedirectToAction("Index");
        }
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Match(bool like)
        {
            if (TempData.ContainsKey("LoginMatch"))
            {
                string? login = (string)TempData["LoginMatch"];
                User? getUserFromMatch;
                if (login != null)
                {
                    getUserFromMatch = await _userDb.GetUserByLogin(login);
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
                User? currentUser = await _userDb.GetUserByLogin(currentUsername);
                if (currentUser == null)
                    return RedirectToAction("Index");
                if (!await UpDateLike(currentUser, getUserFromMatch, like))
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
        private async Task<bool> CreateNewChat(User user1, User user2)
        {
            Console.WriteLine("Newwwww chaaaaat");
            Console.WriteLine(user1.Login);
            Console.WriteLine(user2.Login);
            Chat? chat = await _db.Chats.FirstOrDefaultAsync(c => c.Users.Any(u => u.Id == user1.Id) && c.Users.Any(u => u.Id == user2.Id));

            //Chat? chat = await _db.Chats.FirstOrDefaultAsync<Chat>(c => c.Users.Contains(user1) && c.Users.Contains(user2));
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
                await _db.AddAsync<Message>(message);
                await _db.AddAsync<Chat>(newChat);
                await _db.SaveChangesAsync();
                Console.WriteLine("SaveChangesAsync");
                return true;
            }
            else return false;
        }
        [NonAction]
        private async Task<bool> addSystemMessage(Chat chat, string text)
        {
            //Chat? chat = await _db.Chats.FirstOrDefaultAsync<Chat>(c => c.Users.Contains(user1) && c.Users.Contains(user2));
            if (chat != null)
            {
                Message message = new CreatingMessage(chat, null, text);
                await _db.AddAsync<Message>(message);
                _db.Update<Chat>(chat);
                await _db.SaveChangesAsync();
                return true;
            }
            else return false;
        }
        [NonAction]
        private async Task<bool> UpDateLike(User user1, User user2, bool firstLikeSecond)
        {
            Console.WriteLine("1132513");
            Like? like = await _db.Likes.FirstOrDefaultAsync<Like>(l => l.User1Id == user1.Id && l.User2Id == user2.Id);
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
                    await _db.SaveChangesAsync();
                    return true;
                }
                like.User1LikesUser2 = firstLikeSecond;
                Console.WriteLine("3253325");

                _db.Likes.Update(like);
                await _db.SaveChangesAsync();
                if (like.User1LikesUser2 && like.User2LikesUser1)
                {
                    Console.WriteLine("Must create a new chat!");
                    CreateNewChat(user1, user2);
                }
                return true;
            }
            else
            {

                like = await _db.Likes.FirstOrDefaultAsync<Like>(l => l.User2Id == user1.Id && l.User1Id == user2.Id);

                if (like != null)
                {
                    if (!like.User1LikesUser2 && !like.User2LikesUser1)
                        return false;
                    if (!firstLikeSecond)
                    {
                        like.User1LikesUser2 = false;
                        like.User2LikesUser1 = false;
                        _db.Likes.Update(like);
                        await _db.SaveChangesAsync();
                        return true;
                    }
                    like.User2LikesUser1 = firstLikeSecond;
                    _db.Likes.Update(like);
                    await _db.SaveChangesAsync();
                    Console.WriteLine("Save all changee");
                    if (like.User1LikesUser2 && like.User2LikesUser1)
                    {
                        Console.WriteLine("Must create a new chat!");
                        await CreateNewChat(user1, user2);
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
                    await _db.Likes.AddAsync(like);
                    await _db.SaveChangesAsync();
                    return true;
                }
            }
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Settings()
        {
            string? username;
            UserInfo? info = null;
            if (GetUsernameFromCookie(out username))
            {
                info = await _db.Infos.FirstOrDefaultAsync<UserInfo>(i => i.User.Login == username);
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
                User? user = await _userDb.GetUserByLogin(username);
                bool isUserModif = false, isInfoModif = false;
                UserInfo? userInfo = null;
                Game? game = null;
                if (user != null)
                    userInfo = await _db.Infos.FirstOrDefaultAsync<UserInfo>(i => i.User.Id == user.Id);
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
                        if (userInfo.Age != age)
                        {
                            userInfo.Age = age;
                            isInfoModif = true;
                        }
                        if (userInfo.Name != name)
                        {
                            userInfo.Name = name;
                            isInfoModif = true;
                        }
                        if (userInfo.Description != description)
                        {
                            userInfo.Description = description;
                            isInfoModif = true;
                        }
                        if (userInfo.Contact != contact)
                        {
                            userInfo.Contact = contact;
                            isInfoModif = true;
                        }
                        ICollection<Game>? games = await _db.Games.Where<Game>(g => g.Users.Any(u => u.Id == user.Id)).ToListAsync();
                        if (games == null)
                        {
                            Console.WriteLine("hehhehehe");
                            games = new List<Game>();
                            user.Games = games;
                            games.Add(game);
                            isUserModif = true;
                        }
                        else
                        {
                            if (selectGame != 1000000)
                            {
                                if (!(games.Contains(game)))
                                {
                                    foreach (Game g in games)
                                    {
                                        Console.WriteLine(g.Id);
                                        Console.WriteLine("Айді");
                                    }
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
                            else
                                isUserModif = false;

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
                        if (isInfoModif || isUserModif)
                        {
                            Console.WriteLine("Я тут збережу");
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            Console.WriteLine("збережуж");
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Console.WriteLine("Біда");
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

                        User? addingUser = await _userDb.GetUserByLogin(username);
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
                User? user = await _userDb.GetUserByLogin(username);
                if (user != null)
                {
                    UserInfo? info = user.Info;
                    Console.WriteLine("May be problem");
                    IEnumerable<Like>? likes = await _db.Likes.Where(l => l.User2Id == user.Id).ToListAsync();
                    if (likes.Any())
                    {
                        int? userId = likes.FirstOrDefault(l => l.User1LikesUser2 == true && l.User2LikesUser1 == false)?.User1Id;
                        if (userId == null)
                        {
                            Console.WriteLine("May be problems1");
                            likes = await _db.Likes.Where(l => l.User1Id == user.Id).ToListAsync();
                            userId = likes.FirstOrDefault(l => l.User2LikesUser1 == true && l.User1LikesUser2 == false)?.User2Id;
                        }
                        if (userId != null)
                        {
                            Console.WriteLine("May be problem2");
                            return await _userDb.GetUserById(userId);
                        }
                    }
                    ICollection<Game> games = await _db.Games.Include(c => c.Genre).Where<Game>(g => g.Users.Contains(user)).ToListAsync();
                    Console.WriteLine("May be problem3");
                    if (games.Any())
                    {
                        ICollection<Genre> genres = new List<Genre>();
                        Console.WriteLine("May be problem4");
                        foreach (Game g in games)
                        {
                            Console.WriteLine("May be problem5");
                            if (!genres.Contains(g.Genre))
                            {
                                genres.Add(g.Genre);
                                Console.WriteLine("add5");
                                Console.WriteLine(g.Title);
                                Console.WriteLine(g.Genre.Name);
                            }
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
                                    if (g.Games.Any())
                                        Console.WriteLine("Good");
                                    else Console.WriteLine("Bad");
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
                                usersFromGames = await _userDb.GetUsersListWithGame(ge);
                                if (usersFromGames!=null&&usersFromGames.Any())
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
                                else
                                    Console.WriteLine("Badf");
                            }
                            if (usersMatch == null)
                                return null;
                            else
                            {
                                foreach (User returnUser in usersMatch)
                                {
                                    if (await isUserNonInUnlike(user, returnUser))
                                    {
                                        Console.WriteLine("Return user");
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
        private async Task<bool> isMatch(string loginWhoSearches, string loginWhoChecked)
        {
            User? userWhoSearches = await _userDb.GetUserByLogin(loginWhoSearches);
            if (userWhoSearches == null)
                return false;
            User? userWhoChecked = await _userDb.GetUserByLogin(loginWhoChecked);
            if (userWhoChecked == null)
                return false;
            Like? like = await _db.Likes.FirstOrDefaultAsync<Like>(l => l.User1Id == userWhoSearches.Id && l.User2Id == userWhoChecked.Id);
            if (like != null)
            {
                if (like.User1LikesUser2 && like.User2LikesUser1)
                    return true;
            }
            like = await _db.Likes.FirstOrDefaultAsync<Like>(l => l.User2Id == userWhoSearches.Id && l.User1Id == userWhoChecked.Id);
            if (like != null)
            {
                if (like.User1LikesUser2 && like.User2LikesUser1)
                    return true;
            }
            return false;
        }
        [NonAction]
        private async Task<bool> isUserNonInUnlike(User userWhoSearches, User userWhoChecked)
        {
            Like? like = await _db.Likes.FirstOrDefaultAsync<Like>(l => l.User1Id == userWhoSearches.Id && l.User2Id == userWhoChecked.Id);
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
                like = await _db.Likes.FirstOrDefaultAsync<Like>(l => l.User2Id == userWhoSearches.Id && l.User1Id == userWhoChecked.Id);
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
        private async Task<Message>? GetLastMessage(Chat chat)
        {
            Message? message = null;
            if (chat != null)
            {
                message = await _db.Messages.Where(m => m.Chat == chat).OrderBy(m => m.Id).LastOrDefaultAsync();
            }
            return message;
        }
        [HttpPost]
        public async Task<IActionResult> GetGameOptions(string gameName)
        {
            List<Game> games = await _db.Games.Where(game => game.Title.ToLower().Contains(gameName.ToLower())).ToListAsync();
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
        public async Task<IActionResult> AddGenres()
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
                await _db.Genres.AddAsync(genre);
            }
            await _db.SaveChangesAsync();
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