using BL.Interface;
using DAL.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OpticsProject.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _secretKey = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromQuery] int user_id, [FromQuery]string password)
        {
            try
            {
                var isUserExist = await _userService.Login(user_id, password);
                if (isUserExist==null)
                    return Unauthorized();
                string token;
                if (isUserExist.IsManager)
                    token = GenerateToken(user_id.ToString(), "Admin");
                else
                   token = GenerateToken(isUserExist.Id.ToString(), "User");
                return Ok(new { Token = token });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex);

            }

           
        }

        private string GenerateToken(string userId, string role)
        {
            if (string.IsNullOrEmpty(_secretKey) || _secretKey.Length < 16)
            {
                throw new ArgumentException("Invalid secret key. The key must be at least 16 characters long.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role)

            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}