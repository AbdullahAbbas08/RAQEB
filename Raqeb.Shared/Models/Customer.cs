using System.Text.Json.Serialization;

namespace Raqeb.Shared.Models
{
    public class Customer 
    {
        [Key]
        public int ID { get; set; }

        public string Code { get; set; }
        public string NameAr { get; set; }

        public string NameEn { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string LOBSector { get; set; }
        public string LOBGroup { get; set; }
        public int PoolId { get; set; }
        public decimal Balance { get; set; }  // Exposure at Default (EAD)
        public DateTime DateOfDefault { get; set; }
        public decimal LendingInterestRate { get; set; }
        public decimal CostOfRecovery { get; set; }
        public decimal ExposureWeightOfEachRelativePool { get; set; }
        public decimal RecoveryRateOfEachRelativePool { get; set; }
        public Pool Pool { get; set; }

        // 🔹 المبالغ المستردة بعد التعثر
        public ICollection<RecoveryRecord> Recoveries { get; set; }

        // 🔹 الدرجات الشهرية أو السنوية للعميل
        public ICollection<CustomerGrade> Grades { get; set; }

    }



}  