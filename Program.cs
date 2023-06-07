using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using PlayAndConnect.Data;
using PlayAndConnect.Data.Interfaces;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
//Впровадження залежностей
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IUserDb, UserDb>();
builder.Services.AddTransient<IInfoDb, InfoDb>();
builder.Services.AddTransient<IGameDb, GameDb>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 32));
DbContextOptionsBuilder options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseMySql(connectionString, serverVersion)
    .LogTo(Console.WriteLine, LogLevel.Information) //обережно з цим
    .EnableSensitiveDataLogging()                   //тільки для
    .EnableDetailedErrors();                        
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
            .LogTo(Console.WriteLine, LogLevel.Information) //це
        .EnableSensitiveDataLogging()                       //потім 
        .EnableDetailedErrors()                             //видалити
        );//, ServiceLifetime.Singleton);                        //це може викликати проблеми з багатопотоковістю

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.Cookie.Name = "Login";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });
builder.Services.AddResponseCaching();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
