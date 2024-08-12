using Microsoft.AspNetCore.Mvc;
using DAL;
using DAL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BL.Interface;
using Image_encryption.middleWare;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Image_Encryption.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _IUserService;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        public UserController(IUserService userService, IConfiguration configuration)
        {
            _IUserService = userService;
            _secretKey = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }

        [HttpPost]
        //[Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult> AddUser([FromBody] UserDto newUser)
        {
            try
            {
                Console.WriteLine("come here");
                var res = await _IUserService.CreateUser(newUser);
                if (res)
                    return Ok(res);
                return BadRequest(res);
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }

        [Authorize]
        //[Authorize(Policy = "RequireAdminRole")]
        [HttpPut]
        public async Task<ActionResult> UpdateUser([FromBody] UserDto user)
        {
            try
            {
                var res = await _IUserService.UpdateUser(user);
                if (res)
                    return Ok(res);
                return BadRequest(res);
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            try
            {
                var user = await _IUserService.GetUser(id);
                if (user == null)
                    return NotFound("User not found.");
                return Ok(user);
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }
        [Authorize]
        [HttpGet("GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ICollection<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _IUserService.GetAllUsers();
                if (users == null || users.Count == 0)
                {
                    return NotFound("No users found.");
                }
                return Ok(users);
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }
        [Authorize]
        [HttpPut("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ResetPassword([FromQuery] string password1, [FromQuery] string password2, [FromQuery] int id, [FromQuery] string validation = " ")
        {
            try
            {
                if (password1 != password2)
                {
                    return BadRequest("Passwords do not match.");
                }

                // Validate user ID and perform the password reset


                // Perform the password reset operation
                bool result = await _IUserService.ResetPassword(password1, password2, id, validation);

                if (result)
                {
                    return Ok("Password has been reset successfully.");
                }
                else
                {
                    return StatusCode(500, "An error occurred while resetting the password.");
                }
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }
       
        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserDto>> DeleteUser(int id)
        {
            try
            {
                var user = await _IUserService.DeleteUser(id);
                if (user == null)
                    return NotFound("User not found.");
                return Ok(user);
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }
    }
}