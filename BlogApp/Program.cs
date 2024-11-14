using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using BlogApp;
using BlogApp.Models.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

try
{
    logger.Info("Приложение стартует.");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // Конфигурация строки подключения к базе данных
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(connectionString);
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    });

    logger.Info("Конфигурация строки подключения к базе данных выполнена.");

    // Добавление сервисов в контейнер
    builder.Services.AddControllersWithViews();

    // Регистрация сервисов
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IArticleService, ArticleService>();
    builder.Services.AddScoped<ITagService, TagService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<IRoleService, RoleService>();

    logger.Info("Сервисы зарегистрированы.");

    // Настройка аутентификации и авторизации
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
        options.AddPolicy("ModeratorOnly", policy => policy.RequireRole("Moderator"));
        options.AddPolicy("UserOnly", policy => policy.RequireRole("DefaultUser"));
    });

    logger.Info("Настройка аутентификации и авторизации завершена.");

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error"); // Общая страница ошибок
        app.UseHsts();
        logger.Info("Приложение настроено на продакшн режим.");
    }
    else
    {
        logger.Info("Приложение настроено на режим разработки.");
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    // Аутентификация и авторизация
    app.UseAuthentication();
    app.UseAuthorization();

    // Обработка ошибок 403 и 404
    app.UseStatusCodePages(async context =>
    {
        var response = context.HttpContext.Response;
        var statusCode = response.StatusCode;
        if (statusCode == 403)
        {
            logger.Warn("Ошибка 403: Доступ запрещен.");
            response.Redirect("/Account/AccessDenied");
        }
        else if (statusCode == 404)
        {
            logger.Warn("Ошибка 404: Страница не найдена.");
            response.Redirect("/Home/NotFound");
        }
    });

    // Определение маршрутов
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    logger.Info("Маршруты определены.");

    app.Run();

    logger.Info("Приложение запущено.");
}
catch (Exception ex)
{
    // Логируем фатальные ошибки и выбрасываем исключение
    logger.Error(ex, "Приложение остановилось из-за ошибки");
    throw;
}
finally
{
    // Очищаем NLog
    LogManager.Shutdown();
}
