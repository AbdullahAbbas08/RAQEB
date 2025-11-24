using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raqeb.Shared.Models
{
    public class PDObservedRate
    {
        [Key]
        public int Id { get; set; }

        public int PoolId { get; set; }

        public int Year { get; set; }

        public double ObservedDefaultRate { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
