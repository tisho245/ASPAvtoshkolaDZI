namespace Avtoshkola_DZI.Models
{
    public class FilterModel
    {
        public string? SearchTerm { get; set; }
        public string? CategoryFilter { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
