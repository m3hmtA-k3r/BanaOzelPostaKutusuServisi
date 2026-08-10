namespace PostaKutusuServisi.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalMessages { get; set; }
        public int TodayMessages { get; set; }
        public int UnreadMessages { get; set; }
        public int TrashedMessages { get; set; }
        public int PendingReports { get; set; }


        public List<TopItemViewModel> TopSenders { get; set; }
        public List<TopItemViewModel> TopCategories { get; set; }
    }

    public class TopItemViewModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
