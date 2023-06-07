using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PlayAndConnect.Controllers;
using PlayAndConnect.Data;
using PlayAndConnect.Data.Interfaces;
using Microsoft.Extensions.FileProviders;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;


namespace PlayAndConnect.Tests
{
    public class MockWebHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; }
    public string WebRootPath { get; set; }
    public IFileProvider WebRootFileProvider { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }

    public MockWebHostEnvironment()
    {
        EnvironmentName = "Development"; // Встановіть відповідне значення для середовища, яке ви використовуєте
        ApplicationName = "YourApplicationName"; // Встановіть назву вашого додатка
        WebRootPath = "C:/Users/Олег/Desktop/PlayAndConnect/wwwroot"; // Шлях до папки wwwroot
        WebRootFileProvider = new PhysicalFileProvider(WebRootPath);
        ContentRootPath = "C:/Users/Олег/Desktop/PlayAndConnect/wwwroot"; // Шлях до кореневої папки проекту
        ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
    }
}
}