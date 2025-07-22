using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGOPlatformWeb.Services
{
    public class AchievementService
    {
        private readonly NGODbContext _context;
        
        public AchievementService(NGODbContext context)
        {
            _context = context;
        }

        // 成就定義 - 前端邏輯在這裡定義
        public static readonly Dictionary<string, AchievementDefinition> ACHIEVEMENTS = new()
        {
            ["first_registration"] = new AchievementDefinition
            {
                Code = "first_registration",
                Name = "初來乍到",
                Description = "完成第一次活動報名",
                Icon = "star"
            },
            ["first_completion"] = new AchievementDefinition
            {
                Code = "first_completion", 
                Name = "初體驗",
                Description = "完成第一次活動參與",
                Icon = "check-circle"
            },
            ["multi_category"] = new AchievementDefinition
            {
                Code = "multi_category",
                Name = "多元探索者", 
                Description = "參與3種不同類型的活動",
                Icon = "rainbow"
            },
            ["regular_participant"] = new AchievementDefinition
            {
                Code = "regular_participant",
                Name = "熱心志工",
                Description = "累計完成5次活動",
                Icon = "heart"
            },
            ["case_helper"] = new AchievementDefinition
            {
                Code = "case_helper",
                Name = "溫暖陪伴",
                Description = "參與個案專屬活動", 
                Icon = "users"
            }
        };

        /// <summary>
        /// 檢查並獎勵新成就
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>新獲得的成就代碼列表</returns>
        public async Task<List<string>> CheckAndAwardAchievements(int userId)
        {
            var newAchievements = new List<string>();
            
            // 獲取用戶已有的成就
            var existingAchievements = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementCode)
                .ToListAsync();

            // 檢查每個成就條件
            foreach (var achievement in ACHIEVEMENTS.Values)
            {
                if (!existingAchievements.Contains(achievement.Code) && 
                    await MeetsAchievementCondition(userId, achievement.Code))
                {
                    await AwardAchievement(userId, achievement.Code);
                    newAchievements.Add(achievement.Code);
                }
            }

            return newAchievements;
        }

        /// <summary>
        /// 獲取用戶所有成就
        /// </summary>
        public async Task<List<UserAchievementViewModel>> GetUserAchievements(int userId)
        {
            var earnedAchievements = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToListAsync();

            var result = new List<UserAchievementViewModel>();

            foreach (var achievement in ACHIEVEMENTS.Values)
            {
                var earned = earnedAchievements.FirstOrDefault(ea => ea.AchievementCode == achievement.Code);
                
                result.Add(new UserAchievementViewModel
                {
                    Code = achievement.Code,
                    Name = achievement.Name,
                    Description = achievement.Description,
                    Icon = achievement.Icon,
                    IsEarned = earned != null,
                    EarnedAt = earned?.EarnedAt,
                    IsNew = earned != null && earned.EarnedAt > DateTime.Now.AddHours(-24) // 24小時內獲得算新的
                });
            }

            return result.OrderByDescending(a => a.IsEarned).ThenBy(a => a.Name).ToList();
        }

        /// <summary>
        /// 檢查是否滿足成就條件
        /// </summary>
        private async Task<bool> MeetsAchievementCondition(int userId, string achievementCode)
        {
            return achievementCode switch
            {
                "first_registration" => await HasFirstRegistration(userId),
                "first_completion" => await HasFirstCompletion(userId),
                "multi_category" => await HasMultiCategory(userId),
                "regular_participant" => await HasRegularParticipation(userId),
                "case_helper" => await HasCaseHelperActivity(userId),
                _ => false
            };
        }

        /// <summary>
        /// 獎勵成就
        /// </summary>
        private async Task AwardAchievement(int userId, string achievementCode)
        {
            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementCode = achievementCode,
                EarnedAt = DateTime.Now
            };

            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync();
        }

        // 成就條件檢查方法
        private async Task<bool> HasFirstRegistration(int userId)
        {
            return await _context.UserActivityRegistrations
                .AnyAsync(uar => uar.UserId == userId);
        }

        private async Task<bool> HasFirstCompletion(int userId)
        {
            return await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a)
                .AnyAsync(a => a.Status == "completed" && a.EndDate < DateTime.Now);
        }

        private async Task<bool> HasMultiCategory(int userId)
        {
            var categories = await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a.Category)
                .Distinct()
                .CountAsync();
            
            return categories >= 3;
        }

        private async Task<bool> HasRegularParticipation(int userId)
        {
            var completedCount = await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a)
                .CountAsync(a => a.Status == "completed" && a.EndDate < DateTime.Now);
            
            return completedCount >= 5;
        }

        private async Task<bool> HasCaseHelperActivity(int userId)
        {
            return await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a)
                .AnyAsync(a => a.TargetAudience == "case");
        }
    }

    // 成就定義類別
    public class AchievementDefinition
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    // 用戶成就視圖模型
    public class UserAchievementViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsEarned { get; set; }
        public DateTime? EarnedAt { get; set; }
        public bool IsNew { get; set; }
    }
}