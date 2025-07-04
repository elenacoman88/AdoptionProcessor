namespace RabbitAdoption.ProducerAPI.Models.DTO
{
    public class AdoptionStatusResponseDTO
    {
        public int RequestId { get; set; }
        public string Status { get; set; }
        public int? MatchedRabbitId { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
