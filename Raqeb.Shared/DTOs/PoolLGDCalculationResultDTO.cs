namespace Raqeb.Shared.DTOs
{
 public class PoolLGDCalculationResultDTO
 {
 public int Version { get; set; }
 public List<PoolLGDDTO> Pools { get; set; } = new List<PoolLGDDTO>();
 }
}
