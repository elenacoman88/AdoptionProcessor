using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitAdoption.WorkerService.Models
{
    public class Rabbit
    {
        [Key]
        public int Id { get; set; }
        public string Size { get; set; }      // e.g., "Small", "Medium", "Large"
        public string Color { get; set; }     // e.g., "White", "Brown"
        public int? Age { get; set; }
        public string Status { get; set; }          // e.g. "Available", "Adopted"
        //public DateTime UpdatedAt { get; set; }
    }
}
