namespace Raqeb.Shared.Models
{
    public class Pool
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // الحسابات المجمعة
        public decimal TotalEAD { get; set; }
        public decimal RecoveryRate { get; set; }
        public decimal UnsecuredLGD { get; set; }

        public ICollection<Customer> Customers { get; set; }
    }



}