namespace Raqeb.Shared.Models
{
    public class PoolLGDResult
    {
        [Key]
        public int Id { get; set; }
        public int Version { get; set; }

        [ForeignKey("Pool")]
        public int PoolId { get; set; }

        [MaxLength(200)]
        public string PoolName { get; set; } = string.Empty;

        public decimal EAD { get; set; }

        public decimal RecoveryRate { get; set; }

        public decimal LGD { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
