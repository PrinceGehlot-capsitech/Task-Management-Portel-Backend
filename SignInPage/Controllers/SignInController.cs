using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignInPage.Models;
using SignInPage.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace SignInPage.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SignInController : ControllerBase
    {
        private readonly SignInService _signInService;

        public SignInController(SignInService signInService)
        {
            _signInService = signInService;
        }

        [AllowAnonymous]
        [Route("authenticate")]
        [HttpPost]
        public ActionResult Authenticate([FromBody]SignInDetails signInDetails)
        {
            var token = _signInService.Authenticate(signInDetails.email, signInDetails.password);
            var data = _signInService.GetEmailAsync(signInDetails.email);
            var Id = data.Result?.Id;

            if (token == null)
            {
                return Unauthorized();
            }
            var bearertoken = GenerateToken(signInDetails);
            

            return Ok(new { bearertoken , Id });
            
        }

        
        private string GenerateToken(SignInDetails signInDetails)
        {
            var claims = new[] {
        //new Claim(ClaimTypes.Name, signInDetails.userName),
        new Claim(ClaimTypes.Email, signInDetails.email),
        //new Claim(ClaimTypes.NameIdentifier, signInDetails.Id.ToString())
    };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("your_secret_key_here_with_at_least_256_bits"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds 
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post(SignInDetails newsignInDetails)
        {
            await _signInService.CreateAsync(newsignInDetails);

            try
            {
                return CreatedAtAction(nameof(Get), new { id = newsignInDetails.Id }, newsignInDetails);

            }
            catch (Exception ex) { 
                return BadRequest(ex);
            }
            
        }

        
        [HttpGet]
        public async Task<List<SignInDetails>> Get()
        {
            var signindetails = await _signInService.GetAsync();
            return signindetails;
        }
        [AllowAnonymous]

        [HttpGet ("GetDetails/{Id}")]
        public async Task<SignInDetails> GetById(string Id)
        {
            var signInDetails = await _signInService.GetAsync(Id);
            return signInDetails;
        }
    }
}
