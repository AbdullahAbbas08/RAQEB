namespace Raqeb.Shared.Models
{
    public class Group :_Model
    {
        [Required]
        [MaxLength(150)]
        public string NameAr { get; set; }

        [Required]
        [MaxLength(150)]
        public string NameEn { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }

}  