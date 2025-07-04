using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitAdoption.ProducerAPI.Data;
using RabbitAdoption.ProducerAPI.Models;
using RabbitAdoption.ProducerAPI.Models.DTO;
using RabbitAdoption.ProducerAPI.RabbitMQSender;

namespace RabbitAdoption.ProducerAPI.Controllers
{
    [ApiController]
    [Route("api/adoption-request")]
    public class AdoptionRequestController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ResponseDTO _response;
        private IMapper _mapper;
        private readonly IRabbitMQMessageSender _messageSender;
        private readonly IConfiguration _configuration;

        public AdoptionRequestController(ApplicationDbContext db, IMapper mapper, IRabbitMQMessageSender messageSender, IConfiguration configuration)
        {
            _db = db;
            _response = new ResponseDTO();
            _mapper = mapper;
            _messageSender = messageSender;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetAdoptionRequest")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //method to interogate the status of a request
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                AdoptionRequest obj = await _db.AdoptionRequests.FirstOrDefaultAsync(u => u.RequestId == id);
                _response.Result = _mapper.Map<AdoptionStatusResponseDTO>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitRequest([FromBody] AdoptionRequestDTO adoptionRequestDTO)
        {
            if (adoptionRequestDTO == null)
                return BadRequest("Request body is missing.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Map DTO to entity
                AdoptionRequest adoptionRequest = _mapper.Map<AdoptionRequest>(adoptionRequestDTO);
                if (adoptionRequest == null)
                    return BadRequest("Failed to map request data.");

                // Save to DB
                await _db.AdoptionRequests.AddAsync(adoptionRequest);
                await _db.SaveChangesAsync();

                //build response
                adoptionRequestDTO.RequestId = adoptionRequest.RequestId;
                _response.Result = _mapper.Map<AdoptionRequestDTO>(adoptionRequest);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return StatusCode(500, _response);
            }

            // Get queue name from config
            string? queueName = _configuration.GetValue<string>("TopicAndQueueNames:AdoptionQueue");
            if (string.IsNullOrWhiteSpace(queueName))
            {
                _response.IsSuccess = false;
                _response.Message = "Queue configuration is missing.";
                return StatusCode(500, _response);
            }

            // Send message
            try
            {
                _messageSender.SendMessage(adoptionRequestDTO.RequestId, queueName, (byte)adoptionRequestDTO.Priority);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"Message sending failed: {ex.Message}";
                return StatusCode(500, _response);
            }

            //_messageSender.SendMessage(adoptionRequestDTO.RequestId, _configuration.GetValue<string>("TopicAndQueueNames:AdoptionQueue"));
            return CreatedAtRoute("GetAdoptionRequest", new { id = adoptionRequestDTO.RequestId }, _response);
        }
    }
}
