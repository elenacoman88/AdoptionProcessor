using System.ComponentModel.DataAnnotations;

namespace RabbitAdoption.ProducerAPI.Models.DTO
{
    public class AdoptionRequestDTO
    {
        public int RequestId { get; set; }
        public string AdopterName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string PreferredSize { get; set; }
        public string PreferredColor { get; set; }
        public int? PreferredAge { get; set; }
        public PriorityLevel Priority { get; set; }
        //public string Status { get; set; } = "Pending";
        //public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        //public Guid? MatchedRabbitId { get; set; } 


    }
}
