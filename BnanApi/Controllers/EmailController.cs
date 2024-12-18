using BnanApi.DTOS;
using BnanApi.Services.Email;
using Microsoft.AspNetCore.Mvc;

namespace BnanApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : Controller
    {
        private readonly IMailingService _mailingService;

        public EmailController(IMailingService mailingService)
        {
            _mailingService = mailingService;
        }


        [HttpPost]
        public async Task<IActionResult> SendEmail([FromForm] EmailDTO request)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return 400 Bad Request along with validation errors
                return BadRequest(ModelState);
            }
            if (!_mailingService.IsValidEmail(request.Email))
            {
                return BadRequest("Invalid email address.");
            }
            bool emailToBnanSent = await _mailingService.SendEmailToBnan(request);
            bool emailForCustomerSent = await _mailingService.SendEmailForCustomer(request.Email, request.Name);
            if (emailToBnanSent && emailForCustomerSent)
            {
                return Ok("Sent Successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to send one or more emails.");
            }



        }
    }
}
