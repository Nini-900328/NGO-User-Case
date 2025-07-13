using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NGOPlatformWeb.Services;

namespace NGOPlatformWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly NGODbContext _context;
        private readonly EmailService _emailService;

        public AuthController(NGODbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == vm.Email && u.Password == vm.Password);
            if (user != null)
            {
                await SignInAsync(
                    httpContext: HttpContext,
                    email: user.Email ?? "",
                    id: user.UserId.ToString(),
                    name: user.Name ?? "使用者",
                    role: "User"
                );

                return RedirectToAction("Index", "Home");
            }

            // 2) 嘗試個案登入
            var caseLogin = await _context.CaseLogins
                .FirstOrDefaultAsync(c => c.Email == vm.Email && c.Password == vm.Password);
            if (caseLogin != null)
            {
                var caseName = await _context.Cases
                    .Where(c => c.CaseId == caseLogin.CaseId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync() ?? "個案";

                await SignInAsync(
                    httpContext: HttpContext,
                    email: caseLogin.Email ?? "",
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

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 檢查 Email 是否已存在
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == vm.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "此電子信箱已被註冊");
                return View(vm);
            }

            // 檢查身份證字號是否已存在
            var existingIdentity = _context.Users.FirstOrDefault(u => u.IdentityNumber == vm.IdentityNumber);
            if (existingIdentity != null)
            {
                ModelState.AddModelError(nameof(vm.IdentityNumber), "此身份證字號已被註冊");
                return View(vm);
            }

            // 創建新使用者
            var newUser = new User
            {
                Name = vm.Name,
                Email = vm.Email,
                Password = vm.Password,
                Phone = vm.Phone,
                IdentityNumber = vm.IdentityNumber
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 自動登入
            await SignInAsync(
                httpContext: HttpContext,
                email: newUser.Email,
                id: newUser.UserId.ToString(),
                name: newUser.Name,
                role: "User"
            );

            TempData["SuccessMessage"] = "註冊成功！歡迎加入恩舉平台。";
            return RedirectToAction("Index", "Home");
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

        // Show the forgot password page
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            // Return the forgot password form view
            return View(new ForgotPasswordViewModel());
        }

        // Handles forgot password requests
        // model: Form data containing Email
        // Returns: Redirects to confirmation page
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Check if Email exists in Users or CaseLogins table
                var isUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
                var isCase = await _context.CaseLogins.AnyAsync(c => c.Email == model.Email);

                if (!isUser && !isCase)
                {
                    // Show the same message even if Email does not exist (security consideration)
                    TempData["EmailSent"] = true;
                    return View(model);
                }

                //  Check if there is an unexpired token (prevent duplicate sending)
                var existingToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.Email == model.Email &&
                                            t.ExpiresAt > DateTime.Now &&
                                            !t.IsUsed);

                if (existingToken != null)
                {
                    // Already sent within 5 minutes, show success message directly
                    TempData["EmailSent"] = true;
                    return View(model);
                }

                // Generate a new token
                var token = new PasswordResetToken
                {
                    Email = model.Email,
                    Token = Guid.NewGuid().ToString(),
                    UserType = isUser ? "User" : "Case",
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(15), // 15分鐘有效期
                    IsUsed = false
                };

                // Save token to database
                _context.PasswordResetTokens.Add(token);
                await _context.SaveChangesAsync();

                // Create password reset link
                var resetLink = Url.Action("ResetPassword", "Auth",
                    new { token = token.Token }, Request.Scheme);

                // Send email using injected EmailService
                await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink);

                // Set success flag and return to the same page to show modal
                TempData["EmailSent"] = true;
                return View(model);
            }
            catch 
            {
                // Log error but do not reveal to user
                ModelState.AddModelError(string.Empty, "An error occurred while sending the password reset email. Please try again");
                return View(model);
            }
        }

        // Display reset password page
        // token: Password reset token
        // Returns: Reset password form page
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            // Validate token validity
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token &&
                                          t.ExpiresAt > DateTime.Now &&
                                          !t.IsUsed);

            if (resetToken == null)
            {
                // Token is invalid or expired
                TempData["ErrorMessage"] = "Password reset link is invalid or expired, please request a new one.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token
            };

            return View(model);
        }


        // Handle password reset
        // model: Form data containing new password and token
        // Returns: Redirect to login page
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Validate token validity
                var resetToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.Token == model.Token &&
                                              t.ExpiresAt > DateTime.Now &&
                                              !t.IsUsed);

                if (resetToken == null)
                {
                    ModelState.AddModelError(string.Empty, "重設密碼連結無效或已過期");
                    return View(model);
                }

                // Update corresponding password based on UserType
                if (resetToken.UserType == "User")
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == resetToken.Email);

                    if (user != null)
                    {
                        user.Password = model.Password; // Should encrypt password in production
                        _context.Users.Update(user);
                    }
                }
                else if (resetToken.UserType == "Case")
                {
                    var caseLogin = await _context.CaseLogins
                        .FirstOrDefaultAsync(c => c.Email == resetToken.Email);

                    if (caseLogin != null)
                    {
                        caseLogin.Password = model.Password;
                        _context.CaseLogins.Update(caseLogin);
                    }
                }

                // Mark token as used
                resetToken.IsUsed = true;
                resetToken.UsedAt = DateTime.Now;
                _context.PasswordResetTokens.Update(resetToken);

                // Save changes
                await _context.SaveChangesAsync();

                // Set success message and redirect to login page
                TempData["SuccessMessage"] = "Password reset successfully, please login with your new password.";
                return RedirectToAction("Login");
            }
            catch 
            {
                ModelState.AddModelError(string.Empty, "An error occurred while resetting password, please try again later");
                return View(model);
            }
        }


    }
}
