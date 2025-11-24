namespace Raqeb.Shared.DTOs
{
    // 🔹 خلية انتقال (من درجة إلى درجة)
    public class TransitionCellDto
    {
        public int FromGrade { get; set; }
        public int ToGrade { get; set; }
        public double Count { get; set; }
    }
}
