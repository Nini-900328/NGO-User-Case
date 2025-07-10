using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models;

namespace NGOPlatformWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                // 輪播圖數據
                CarouselItems = GetCarouselItems(),

                // 功能卡片數據
                FeatureCards = GetFeatureCards(),

                // 新消息數據
                NewsItems = GetNewsItems(),

                // 活動權證數據
                ActivityInfo = GetActivityInfo()
            };

            return View(model);
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "這裡是聯絡我們";
            return View(); // 聯絡我們
        }

        public IActionResult Organization()
        {
            ViewData["Title"] = "這裡是組織介紹";
            return View(); // 組織介紹
        }
        // 模擬數據方法
        private List<CarouselItem> GetCarouselItems()
        {
            return new List<CarouselItem>
    {
        new CarouselItem
        {
            ImageUrl = "https://images.unsplash.com/photo-1559027615-cd4628902d4a?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
            Title = "歡迎來到NGO物資平台",
            Description = "連結NGO組織與資源，共創美好社會"
        },
        new CarouselItem
        {
            ImageUrl = "https://images.unsplash.com/photo-1582213782179-e0d53f98f2ca?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
            Title = "專業社工培訓課程",
            Description = "提升專業技能，服務社會大眾"
        },
        new CarouselItem
        {
            ImageUrl = "https://images.unsplash.com/photo-1469571486292-0ba58a3f068b?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
            Title = "志工招募中",
            Description = "加入我們，一起為社會貢獻力量"
        }
    };
        }

        private List<FeatureCard> GetFeatureCards()
        {
            return new List<FeatureCard>
    {
        new FeatureCard
        {
            Title = "報名證照",
            Description = "專業證照課程報名與管理",
            IconUrl = "/images/icon1.png"
        },
        new FeatureCard
        {
            Title = "活動管理",
            Description = "志工活動與社區服務",
            IconUrl = "/images/icon2.png"
        },
        new FeatureCard
        {
            Title = "組織介紹",
            Description = "了解各NGO組織資訊",
            IconUrl = "/images/icon3.png"
        },
        new FeatureCard
        {
            Title = "智庫資料",
            Description = "專業知識與資源分享",
            IconUrl = "/images/icon4.png"
        }
    };
        }

        private List<NewsItem> GetNewsItems()
        {
            return new List<NewsItem>
    {
        new NewsItem
        {
            Title = "2024年社工專業認證開始報名",
            Date = DateTime.Now.AddDays(-1),
            Content = "即日起開放報名，名額有限請把握機會"
        },
        new NewsItem
        {
            Title = "新增線上課程功能",
            Date = DateTime.Now.AddDays(-2),
            Content = "現在可以透過線上方式參與課程學習"
        }
    };
        }

        private ActivityInfo GetActivityInfo()
        {
            return new ActivityInfo
            {
                Title = "活動權證",
                Description = "參與社區服務活動，獲得服務時數認證，累積社會服務經驗。透過實際參與，培養社會責任感，為社區帶來正面影響。",
                StartDate = DateTime.Now.AddDays(7),
                ImageUrl = "https://images.unsplash.com/photo-1544027993-37dbfe43562a?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=800&q=80"
            };
        }

    }
}
