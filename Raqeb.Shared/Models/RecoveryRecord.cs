namespace Raqeb.Shared.Models
{
    //يمثل المبلغ المسترد من العميل في سنة معينة بعد التعثر
    public class RecoveryRecord
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int Year { get; set; }
        public decimal RecoveryAmount { get; set; }

        // 🔹 تكلفة التحصيل لهذه السنة فقط
        public decimal RecoveryCost { get; set; }
    }
}
