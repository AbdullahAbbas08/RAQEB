namespace Raqeb.Shared.DTOs
{
    // 🔹 نتيجة مجزأة (Pagination)
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }



    public class TransitionMatrixDto
    {
        public int Year { get; set; }
        public int? Month { get; set; }
        public bool IsYearlyAverage { get; set; }
        public string Title { get; set; }
        public List<TransitionCellDto> Cells { get; set; }
        public List<RowStatDto> RowStats { get; set; }
    }


  
}
