namespace Raqeb.Shared.DTOs
{
    // 🔹 مصفوفة الانتقال الكاملة لشهر محدد
    public class PDTransitionMatrixDto
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public List<TransitionCellDto> Cells { get; set; } = new();
        public List<RowStatDto> RowStats { get; set; } = new();
    }
}
