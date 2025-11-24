namespace Raqeb.Shared.Models
{
    public class Country:_Model
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public virtual Region Region { get; set; }
    }



}