using AutoMapper;
using RabbitAdoption.ProducerAPI.Models;
using RabbitAdoption.ProducerAPI.Models.DTO;

namespace RabbitAdoption.ProducerAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<AdoptionRequest, AdoptionRequestDTO>().ReverseMap();
                config.CreateMap<AdoptionRequest, AdoptionStatusResponseDTO>().ReverseMap();

            });
            return mappingConfig;
        }
    }
}
