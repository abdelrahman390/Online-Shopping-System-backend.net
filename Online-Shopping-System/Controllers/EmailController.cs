using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Online_Shopping_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestEmail()
        {
            await _emailService.SendEmailAsync(
                "abdelrahmanbo390@gmail.com",
                "Test Email",
                "Hello from my Online Shopping System!"
            );

            return Ok("Email sent!");
        }
    }
}
