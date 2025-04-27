using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ApplicationDbContext context;

        public ProfileController(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var profile = new
            {
                user.Role,
                user.FirstName,
                user.LastName,
                user.Email,
                user.DateOfBirth,
                user.Address,
                user.Phone,
                user.Major,
                user.Status,
                user.JoinedAt,
                user.Bio,
                user.PictureURL
            };

            return Ok(profile);
        }

        [HttpPatch("edit-profile")]
        [Authorize]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileDto dto)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized("User not found.");

            //user.FirstName = dto.FirstName;
            //user.LastName = dto.LastName;
            //user.Email = dto.Email;
            //user.DateOfBirth = dto.DateOfBirth;
            //user.Major = dto.Major;
            //user.Address = dto.Address;
            //user.PictureURL = dto.PictureURL;
            //user.Phone = dto.Phone;
            ////user.Status = dto.Status.Value;
            //if (dto.Status.HasValue)
            //{
            //    user.Status = dto.Status.Value;
            //}
            //user.Bio = dto.Bio;

            //user.FirstName = dto.FirstName ?? user.FirstName;
            //user.LastName = dto.LastName ?? user.LastName;
            //user.Email = dto.Email ?? user.Email;
            //user.DateOfBirth = dto.DateOfBirth ?? user.DateOfBirth;
            //user.Major = dto.Major ?? user.Major;
            //user.Address = dto.Address ?? user.Address;
            //user.PictureURL = dto.PictureURL ?? user.PictureURL;
            //user.Phone = dto.Phone ?? user.Phone;
            //user.Status = dto.Status ?? user.Status;
            //user.Bio = dto.Bio ?? user.Bio;

            user.FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? user.FirstName : dto.FirstName;
            user.LastName = string.IsNullOrWhiteSpace(dto.LastName) ? user.LastName : dto.LastName;
            user.Email = string.IsNullOrWhiteSpace(dto.Email) ? user.Email : dto.Email;
            user.DateOfBirth = dto.DateOfBirth ?? user.DateOfBirth;
            user.Major = string.IsNullOrWhiteSpace(dto.Major) ? user.Major : dto.Major;
            user.Address = string.IsNullOrWhiteSpace(dto.Address) ? user.Address : dto.Address;
            user.PictureURL = string.IsNullOrWhiteSpace(dto.PictureURL) ? user.PictureURL : dto.PictureURL;
            user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? user.Phone : dto.Phone;
            user.Status = dto.Status ?? user.Status;
            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? user.Bio : dto.Bio;


            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest("Failed to update profile.");

            return Ok("Profile updated successfully.");
        }

    }
}
