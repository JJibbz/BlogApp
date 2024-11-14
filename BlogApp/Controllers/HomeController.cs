using BlogApp.Models;
using BlogApp.Models.Services;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ITagService _tagService;
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;
        private readonly ApplicationDbContext _context;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HomeController(
            ILogger<HomeController> logger,
            IUserService userService,
            IRoleService roleService,
            ITagService tagService,
            IArticleService articleService,
            ICommentService commentService,
            ApplicationDbContext context
            )
        {
            _logger = logger;
            _userService = userService;
            _roleService = roleService;
            _tagService = tagService;
            _articleService = articleService;
            _commentService = commentService;
            _context = context;
        }

        // ������� �� ������� �������� � ��������������� �� �������� ������
        public IActionResult Index()
        {
            Logger.Info("������� �� ������� ��������. ��������������� �� �������� �� ��������.");
            return RedirectToAction("Articles");
        }

        // GET: Home/Comments
        public async Task<IActionResult> Comments()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            Logger.Info("�������� �������� �� ����� �������������.");
            return View(comments);
        }

        // GET: Home/Tags
        public async Task<IActionResult> Tags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            var tagsWithArticleCount = tags.Select(tag => new
            {
                Tag = tag,
                ArticleCount = _context.Articles.Count(a => a.Tags.Any(t => t.TagId == tag.TagId))
            }).ToList();

            Logger.Info("�������� �������� � ������.");
            return View(tagsWithArticleCount);
        }

        // GET: Home/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            foreach (var user in users)
            {
                user.Role = await _roleService.GetRoleByIdAsync(user.RoleId); // �������� ���� ��� ������������
            }

            Logger.Info("�������� �������� �� ����� ��������������.");
            return View(users);
        }

        // GET: Home/Roles
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            Logger.Info("�������� �������� �� ����� ������.");
            return View(roles);
        }

        // GET: Home/Articles
        public async Task<IActionResult> Articles()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            Logger.Info("�������� �������� �� ����� ��������.");
            return View(articles);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Logger.Error("��������� ������. ������ ID: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            Logger.Warn("������� ������� � ������������ ��������.");
            return View();
        }

        public IActionResult NotFound()
        {
            Logger.Warn("�������� �� �������.");
            return View();
        }
    }
}
