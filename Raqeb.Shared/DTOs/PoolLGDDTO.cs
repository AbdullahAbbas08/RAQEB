namespace Raqeb.Shared.DTOs
{
    public class PoolLGDDTO
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; }
        public decimal EAD { get; set; }              // مجموع القرض (Exposure)
        public decimal RecoveryRate { get; set; }     // نسبة التحصيل بعد الخصم
        public decimal UnsecuredLGD { get; set; }     // نسبة الخسارة الفعلية
    }

}
