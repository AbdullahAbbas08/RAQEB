namespace Raqeb.Shared.Models
{
    public sealed class TransitionCountsResult
    {
        public int[,] Counts { get; }
        public int[] RowTotals { get; }
        public double[] RowPD { get; }  // نسبة التحول إلى Grade الافتراضي (عادةً آخر Grade)
        public int MinGrade { get; }
        public int MaxGrade { get; }

        public TransitionCountsResult(int[,] counts, int[] rowTotals, double[] rowPd, int minGrade, int maxGrade)
        {
            Counts = counts;
            RowTotals = rowTotals;
            RowPD = rowPd;
            MinGrade = minGrade;
            MaxGrade = maxGrade;
        }
    }

}
