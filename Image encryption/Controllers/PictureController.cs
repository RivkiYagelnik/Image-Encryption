
using BL.Interface;
using DAL.DTO;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using DAL;
using BL;
using BL.Services;
using DAL.DTO;
using Microsoft.AspNetCore.Authorization;

namespace Image_Encryption.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PictureController : Controller
    {
        private readonly IPictureService _IPictureService;
        public PictureController(IPictureService IPictureService)
        {
            _IPictureService = IPictureService;
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> AddPicture([FromQuery] string inputPath, [FromQuery] string message, [FromQuery] int id, [FromQuery] byte[]? key = null, [FromQuery] byte[]? iv = null)
        {
            try
            {
                bool res;
                if (key == null)
                {
                   res =await _IPictureService.CreatePicture(inputPath, message, DateTime.Now, id);
                }
                else
                {
                    res =await _IPictureService.CreatePicture(inputPath, message, key, iv, DateTime.Now, id);
                }
                return Ok(res);
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while adding the picture.");
            }
        }
        //[HttpPost]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<bool>> AddPicture([FromQuery] string inputPath, [FromQuery] string message, [FromQuery] int id, [FromQuery] byte[]? key = null, [FromQuery] byte[]? iv = null)
        //{
        //    try
        //    {
        //        Task<bool> res;
        //        if (key == null)
        //        {
        //            res = _IPictureService.AddPicture(inputPath, message, DateTime.Now, id);
        //        }
        //        else
        //        {
        //            res = _IPictureService.AddPicture(inputPath, message, key, iv, DateTime.Now, id);
        //        }
        //        return Ok(await res);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (ex)
        //        Console.WriteLine(ex.Message);
        //        Console.WriteLine(ex.StackTrace);
        //        return StatusCode(500, "An error occurred while adding the picture.");
        //    }
        //}
        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> DeletePicture(int id)
        {
            try
            {
                var res = await _IPictureService.DeletePicture(id);
                return Ok(res);
            }
            catch (Exception ex)
            {

                return BadRequest(ex);
            }
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PictureDto>> GetPictureById(int id)
        {
            try
            {
                var res = await _IPictureService.GetPicture(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<List<PictureDto>>> GetAllPicturesByUser(int userId)
        {
            try
            {
                var pictures = await _IPictureService.GetAllPicture(userId);
                if (pictures == null || pictures.Count == 0)
                {
                    return NotFound("No pictures found for the user.");
                }
                return Ok(pictures);
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while retrieving pictures for the user.");
            }
        }

        // GET: api/Picture/Key
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("Key")]
        public async Task<ActionResult<List<PictureDto>>> GetAllPicturesByKey([FromQuery] string key, [FromQuery] string iv)
        {
            try
            {
                var pictures = await _IPictureService.GetAllPicture(key, iv);
                if (pictures == null || pictures.Count == 0)
                {
                    return NotFound("No pictures found for the provided key and IV.");
                }
                return Ok(pictures);
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, "An error occurred while retrieving pictures by key and IV.");
            }
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("DecryptMessageFromImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> DecryptMessageFromImage([FromQuery] int id, [FromQuery] byte[] key, [FromQuery] byte[] iv)
        {
            try
            {
                var res = await _IPictureService.DecryptMessageFromImage(id, key, iv);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}