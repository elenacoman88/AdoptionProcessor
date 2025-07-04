using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RabbitAdoption.ProducerAPI.Models
{

    public class AdoptionRequest
    {
        [Key]
        public int RequestId { get; set; }
        // Adopter Information
        [Required]
        public string AdopterName { get; set; }
        [Required]
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }


        // Preferred Rabbit Attributes
        public string PreferredSize { get; set; }      // e.g., "Small", "Medium", "Large"
        public string PreferredColor { get; set; }     // e.g., "White", "Brown"
        public int? PreferredAge { get; set; }         // in months
                                                       // Priority
        [Required]
        public PriorityLevel Priority { get; set; }

        // Status Tracking
        public string Status { get; set; } = "Pending";  // "Pending", "Matched", "Failed", etc.
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public int? MatchedRabbitId { get; set; }       // set when matched

    }

    public enum PriorityLevel
    {
        Normal = 0,
        Urgent = 1
    }
}
