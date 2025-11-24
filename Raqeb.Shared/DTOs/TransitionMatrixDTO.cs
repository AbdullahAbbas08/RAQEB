namespace Raqeb.Shared.DTOs
{
    public class TransitionMatrixDTO
        {
            public int PoolId { get; set; }
            public string PoolName { get; set; } = string.Empty;
            public double[,] Matrix { get; set; } = new double[0, 0];
            public double[,] AverageMatrix { get; set; } = new double[0, 0];
            public double[,] LongRunMatrix { get; set; } = new double[0, 0];
            public double ObservedDefaultRate { get; set; }
        }
}
