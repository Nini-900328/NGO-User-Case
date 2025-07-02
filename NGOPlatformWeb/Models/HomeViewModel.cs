namespace NGOPlatformWeb.Models

// 到時候用來放首頁所需的數據模型
{
    public class HomeViewModel
    {
        public List<CarouselItem> CarouselItems { get; set; } = new List<CarouselItem>();
        public List<FeatureCard> FeatureCards { get; set; } = new List<FeatureCard>();
        public List<NewsItem> NewsItems { get; set; } = new List<NewsItem>();
        public ActivityInfo ActivityInfo { get; set; } = new ActivityInfo();
    }

    public class CarouselItem
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class FeatureCard
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
    }

    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class ActivityInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
