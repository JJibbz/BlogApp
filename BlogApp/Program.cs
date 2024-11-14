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
    logger.Info("���������� ��������.");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // ������������ ������ ����������� � ���� ������
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(connectionString);
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    });

    logger.Info("������������ ������ ����������� � ���� ������ ���������.");

    // ���������� �������� � ���������
    builder.Services.AddControllersWithViews();

    // ����������� ��������
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IArticleService, ArticleService>();
    builder.Services.AddScoped<ITagService, TagService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<IRoleService, RoleService>();

    logger.Info("������� ����������������.");

    // ��������� �������������� � �����������
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

    logger.Info("��������� �������������� � ����������� ���������.");

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error"); // ����� �������� ������
        app.UseHsts();
        logger.Info("���������� ��������� �� �������� �����.");
    }
    else
    {
        logger.Info("���������� ��������� �� ����� ����������.");
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    // �������������� � �����������
    app.UseAuthentication();
    app.UseAuthorization();

    // ��������� ������ 403 � 404
    app.UseStatusCodePages(async context =>
    {
        var response = context.HttpContext.Response;
        var statusCode = response.StatusCode;
        if (statusCode == 403)
        {
            logger.Warn("������ 403: ������ ��������.");
            response.Redirect("/Account/AccessDenied");
        }
        else if (statusCode == 404)
        {
            logger.Warn("������ 404: �������� �� �������.");
            response.Redirect("/Home/NotFound");
        }
    });

    // ����������� ���������
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    logger.Info("�������� ����������.");

    app.Run();

    logger.Info("���������� ��������.");
}
catch (Exception ex)
{
    // �������� ��������� ������ � ����������� ����������
    logger.Error(ex, "���������� ������������ ��-�� ������");
    throw;
}
finally
{
    // ������� NLog
    LogManager.Shutdown();
}
