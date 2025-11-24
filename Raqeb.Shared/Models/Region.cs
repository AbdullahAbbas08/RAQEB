namespace Raqeb.Shared.Models
{
    public class Region : _Model
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public virtual ICollection<Country> Countries { get; set; }
    }
    


}  