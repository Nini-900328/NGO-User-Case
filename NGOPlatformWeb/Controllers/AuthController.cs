using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NGOPlatformWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly NGODbContext _context;

        public AuthController(NGODbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 1) 嘗試普通使用者
            var user = _context.Users
                .FirstOrDefault(u => u.Email == vm.Email && u.Password == vm.Password);
            if (user != null)
            {
                await SignInAsync(
                    httpContext: HttpContext,
                    email: user.Email,
                    id: user.UserId.ToString(),
                    name: user.Name,
                    role: "User"
                );

                return RedirectToAction("Index", "Home");
            }

            // 2) 嘗試個案登入
            var caseLogin = _context.CaseLogins
                .FirstOrDefault(c => c.Email == vm.Email && c.Password == vm.Password);
            if (caseLogin != null)
            {
                var cas = _context.Cases.Find(caseLogin.CaseId);
                var caseName = cas?.Name ?? "個案";

                await SignInAsync(
                    httpContext: HttpContext,
                    email: caseLogin.Email,
                    id: caseLogin.CaseId.ToString(),
                    name: caseName,
                    role: "Case"
                );

                return RedirectToAction("Index", "Home");
            }

            // 3) 都找不到就回錯誤
            ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
            return View(vm);
        }

        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        /// <summary>
        /// 統一建立 Cookie Authentication，供其他 Controller 共用
        /// </summary>
        public static async Task SignInAsync(HttpContext httpContext, string id, string name, string role, string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email,           email),
                new Claim(ClaimTypes.NameIdentifier,  id),
                new Claim(ClaimTypes.Name,            name),
                new Claim(ClaimTypes.Role,            role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true }
            );
        }
    }
}
