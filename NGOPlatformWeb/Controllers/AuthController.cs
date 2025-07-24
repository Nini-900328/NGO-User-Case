using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using NGOPlatformWeb.Models.ViewModels.Auth;
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
        private readonly PasswordService _passwordService;
        private readonly AchievementService _achievementService;

        public AuthController(NGODbContext context, EmailService emailService, PasswordService passwordService, AchievementService achievementService)
        {
            _context = context;
            _emailService = emailService;
            _passwordService = passwordService;
            _achievementService = achievementService;
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

                // 嘗試一般使用者登入
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
            
            if (user != null && ValidatePassword(vm.Password, user.Password))
            {
                await SignInAsync(
                    httpContext: HttpContext,
                    id: user.UserId.ToString(),
                    name: user.Name ?? "使用者",
                    role: "User",
                    email: user.Email ?? ""
                );

                return RedirectToAction("Index", "Home");
            }

            // 嘗試個案登入
            var caseLogin = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == vm.Email);
            
            if (caseLogin != null && ValidatePassword(vm.Password, caseLogin.Password))
            {
                var caseName = await _context.Cases
                    .Where(c => c.CaseId == caseLogin.CaseId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync() ?? "個案";

                await SignInAsync(
                    httpContext: HttpContext,
                    id: caseLogin.CaseId.ToString(),
                    name: caseName,
                    role: "Case",
                    email: caseLogin.Email ?? ""
                );

                return RedirectToAction("Index", "Home");
            }

            // 登入失敗
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
                Password = _passwordService.HashPassword(vm.Password),
                Phone = vm.Phone,
                IdentityNumber = vm.IdentityNumber,
                ProfileImage = "/images/user-avatar-circle.svg" // 預設頭像
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 自動登入
            await SignInAsync(
                httpContext: HttpContext,
                id: newUser.UserId.ToString(),
                name: newUser.Name,
                role: "User",
                email: newUser.Email
            );

            // 檢查新用戶成就 (註冊時可能有初始成就)
            try
            {
                var newAchievements = await _achievementService.CheckAndAwardAchievements(newUser.UserId);
                if (newAchievements.Any())
                {
                    TempData["NewAchievements"] = string.Join(",", newAchievements);
                }
            }
            catch
            {
                // 成就檢查失敗不影響註冊流程
            }

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
                        user.Password = _passwordService.HashPassword(model.Password);
                        _context.Users.Update(user);
                    }
                }
                else if (resetToken.UserType == "Case")
                {
                    var caseLogin = await _context.CaseLogins
                        .FirstOrDefaultAsync(c => c.Email == resetToken.Email);

                    if (caseLogin != null)
                    {
                        caseLogin.Password = _passwordService.HashPassword(model.Password);
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

        // POST: /Auth/ExternalLogin
        [HttpPost]
        public IActionResult ExternalLogin(string provider)
        {
            Console.WriteLine($"=== ExternalLogin 開始，Provider: {provider} ===");
            
            // 設定外部登入的回調路徑
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth");
            Console.WriteLine($"回調網址: {redirectUrl}");
            
            var authenticationProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            
            Console.WriteLine("準備跳轉到 Google OAuth...");
            return Challenge(authenticationProperties, provider);
        }

        // GET: /signin-google (Google OAuth 預設回調路徑)
        [HttpGet("/signin-google")]
        public async Task<IActionResult> GoogleCallback()
        {
            return await ExternalLoginCallback();
        }

        // GET: /Auth/ExternalLoginCallback
        public async Task<IActionResult> ExternalLoginCallback()
        {
            try
            {
                // 詳細除錯日誌
                Console.WriteLine("=== ExternalLoginCallback 開始 ===");
                
                // 獲取外部登入資訊
                var info = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
                
                Console.WriteLine($"認證結果: {info.Succeeded}");
                
                if (!info.Succeeded)
                {
                    Console.WriteLine("Google 認證失敗");
                    TempData["ErrorMessage"] = "Google 登入失敗，請重試";
                    return RedirectToAction("Login");
                }

                // 從 Google 獲取用戶資訊
                var email = info.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var name = info.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                
                // 嘗試多種可能的頭像 claim 名稱
                var picture = info.Principal?.FindFirst("picture")?.Value ?? 
                             info.Principal?.FindFirst("urn:google:profile:picture")?.Value ?? 
                             info.Principal?.FindFirst("urn:google:picture")?.Value ??
                             info.Principal?.FindFirst("profile_image")?.Value ??
                             info.Principal?.FindFirst("image")?.Value ??
                             info.Principal?.FindFirst("avatar")?.Value;

                Console.WriteLine($"獲取到的用戶資訊:");
                Console.WriteLine($"Email: {email}");
                Console.WriteLine($"Name: {name}");
                Console.WriteLine($"Picture: {picture}");
                
                // 詳細除錯：列出所有 Google 回傳的 claims
                Console.WriteLine("=== 開始列出所有 Google Claims ===");
                var allClaims = info.Principal?.Claims?.ToList();
                Console.WriteLine($"總共有 {allClaims?.Count ?? 0} 個 claims");
                if (allClaims != null)
                {
                    foreach (var claim in allClaims)
                    {
                        Console.WriteLine($"[CLAIM] Type: '{claim.Type}' | Value: '{claim.Value}'");
                    }
                }
                else
                {
                    Console.WriteLine("沒有任何 claims");
                }
                Console.WriteLine("=== Claims 列表結束 ===");

                if (string.IsNullOrEmpty(email))
                {
                    Console.WriteLine("Email 為空");
                    TempData["ErrorMessage"] = "無法獲取 Google 帳號信箱";
                    return RedirectToAction("Login");
                }

                // 檢查用戶是否已存在於 Users 表
                Console.WriteLine("檢查用戶是否存在...");
                Console.WriteLine($"要檢查的Email: {email}");
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (existingUser == null)
                {
                    Console.WriteLine("用戶不存在，創建新用戶...");
                    
                    // 處理可能過長的資料
                    var userName = name ?? "Google 用戶";
                    if (userName.Length > 50) userName = userName.Substring(0, 50);
                    
                    var profileImageUrl = picture;
                    if (!string.IsNullOrEmpty(profileImageUrl) && profileImageUrl.Length > 255) 
                        profileImageUrl = profileImageUrl.Substring(0, 255);
                    
                    // 如果 Google 沒有提供頭像，設定預設頭像
                    if (string.IsNullOrEmpty(profileImageUrl))
                    {
                        profileImageUrl = "/images/user-avatar-circle.svg"; // 預設 Google OAuth 用戶頭像
                        Console.WriteLine("Google 未提供頭像，使用預設頭像");
                    }
                    else
                    {
                        Console.WriteLine($"Google 提供的頭像: {profileImageUrl}");
                    }
                    
                    var userEmail = email;
                    if (!string.IsNullOrEmpty(userEmail) && userEmail.Length > 100)
                        userEmail = userEmail.Substring(0, 100);
                    
                    Console.WriteLine($"處理後的資料 - Name: {userName}, Email: {userEmail}, ProfileImage: {profileImageUrl}");
                    
                    // 首次登入，自動創建新用戶
                    // 為Google OAuth用戶生成暫時識別碼（15碼，用戶後續需要更新為真實身分證號）
                    var tempIdentityNumber = $"TEMP{DateTime.Now:yyyyMMddHHmmss}";
                    
                    var newUser = new User
                    {
                        Email = userEmail,
                        Name = userName,
                        ProfileImage = profileImageUrl,
                        Password = "GOOGLE_OAUTH_USER", // 第三方登入用戶的特殊標記
                        Phone = null,
                        IdentityNumber = tempIdentityNumber // 暫時識別碼，用戶需要在個人資料頁更新
                    };

                    _context.Users.Add(newUser);
                    Console.WriteLine("正在保存用戶到資料庫...");
                    
                    try
                    {
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"新用戶已創建，ID: {newUser.UserId}");
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"資料庫保存錯誤: {dbEx.Message}");
                        if (dbEx.InnerException != null)
                        {
                            Console.WriteLine($"內部錯誤: {dbEx.InnerException.Message}");
                            if (dbEx.InnerException.InnerException != null)
                            {
                                Console.WriteLine($"深層錯誤: {dbEx.InnerException.InnerException.Message}");
                            }
                        }
                        Console.WriteLine($"StackTrace: {dbEx.StackTrace}");
                        throw; // 重新拋出例外
                    }

                    // 檢查新用戶成就
                    try
                    {
                        var newAchievements = await _achievementService.CheckAndAwardAchievements(newUser.UserId);
                        if (newAchievements.Any())
                        {
                            TempData["NewAchievements"] = string.Join(",", newAchievements);
                        }
                    }
                    catch (Exception achEx)
                    {
                        Console.WriteLine($"成就檢查失敗: {achEx.Message}");
                        // 成就檢查失敗不影響登入流程
                    }

                    existingUser = newUser;
                }
                else
                {
                    Console.WriteLine($"用戶已存在，ID: {existingUser.UserId}");
                }

                // 執行登入
                Console.WriteLine("執行登入...");
                await SignInAsync(
                    httpContext: HttpContext,
                    id: existingUser.UserId.ToString(),
                    name: existingUser.Name ?? "使用者",
                    role: "User",
                    email: existingUser.Email ?? ""
                );

                Console.WriteLine("登入成功，重導向到首頁");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExternalLoginCallback 錯誤: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"登入過程發生錯誤: {ex.Message}";
                return RedirectToAction("Login");
            }
        }

        // 密碼驗證輔助方法 - 支援 BCrypt 和明文密碼
        private bool ValidatePassword(string inputPassword, string storedPassword)
        {
            // Google OAuth 用戶不能用傳統方式登入
            if (storedPassword == "GOOGLE_OAUTH_USER")
            {
                return false;
            }
            
            // BCrypt 加密密碼驗證
            if (storedPassword.StartsWith("$2a$"))
            {
                return _passwordService.VerifyPassword(inputPassword, storedPassword);
            }
            // 向下兼容明文密碼
            return inputPassword == storedPassword;
        }

    }
}
