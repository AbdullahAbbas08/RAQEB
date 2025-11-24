namespace Raqeb.Shared.ViewModels.DTOs
{
    public class PDCalculationDTO
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; }

        // 🔹 كل مصفوفة يتم تمثيلها كـ List<List<double>>
        public List<List<double>> TransitionMatrix { get; set; } = new();
        public List<List<double>> AverageMatrix { get; set; } = new();
        public List<List<double>> LongRunMatrix { get; set; } = new();

        // 🔹 معدل التعثر الفعلي
        public double ObservedDefaultRate { get; set; }

        // 🔹 رقم النسخة
        public int Version { get; set; }
    }
}
