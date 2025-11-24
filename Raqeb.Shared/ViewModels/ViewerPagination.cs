namespace Raqeb.Shared.ViewModels
{
    public class ViewerPagination<T> where T : class
    {
        public List<T> PaginationList { get; set; }
        public int OriginalListListCount { get; set; }

    }
}
