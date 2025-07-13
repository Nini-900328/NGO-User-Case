namespace NGOPlatformWeb.Models
{
    /// <summary>
    /// 首頁的ViewModel
    /// 包含輪播圖、功能卡片、最新消息、活動資訊等首頁元素
    /// </summary>
    public class HomeViewModel
    {
        public List<CarouselItem> CarouselItems { get; set; } = new List<CarouselItem>();
        public List<FeatureCard> FeatureCards { get; set; } = new List<FeatureCard>();
        public List<NewsItem> NewsItems { get; set; } = new List<NewsItem>();
        public ActivityInfo ActivityInfo { get; set; } = new ActivityInfo();
    }

    /// <summary>
    /// 首頁輪播圖的項目
    /// </summary>
    public class CarouselItem
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 首頁功能卡片
    /// </summary>
    public class FeatureCard
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 首頁最新消息項目
    /// </summary>
    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// 首頁活動資訊
    /// </summary>
    public class ActivityInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
