using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raqeb.Shared.Models
{
    public class MacroeconomicInput
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string VariableName { get; set; }

        public int Year { get; set; }

        public double Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
