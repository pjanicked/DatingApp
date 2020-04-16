using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("/api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinarySettings;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,        
            IOptions<CloudinarySettings> cloudinarySettings)
        {
            _cloudinarySettings = cloudinarySettings;
            _mapper = mapper;
            _repo = repo;

            Account acc = new Account(
                _cloudinarySettings.Value.CloudName,
                _cloudinarySettings.Value.ApiKey,
                _cloudinarySettings.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoObj = await _repo.GetPhoto(id);
            var photoReturnDtoObj = _mapper.Map<PhotoForReturnDto>(photoObj);
            return Ok(photoReturnDtoObj);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotosForUser(int userId, [FromForm]PhotoForCreationDto photoCrrationDtoObj)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId);

            var uploadResult = new ImageUploadResult();
            
            var file = photoCrrationDtoObj.File;

            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoCrrationDtoObj.Url = uploadResult.Uri.ToString();
            photoCrrationDtoObj.PublicId = uploadResult.PublicId;

            var photoObj = _mapper.Map<Photo>(photoCrrationDtoObj);

            if(!userFromRepo.Photos.Any(u => u.IsMain))
                photoObj.IsMain = true;

            userFromRepo.Photos.Add(photoObj);

            if(await _repo.SaveAll())
            {
                var photoToReturn = Mapper.Map<PhotoForReturnDto>(photoObj);
                return CreatedAtRoute("GetPhoto", new {id = photoObj.Id}, photoToReturn);
            }

            return BadRequest("Error in adding photos");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId);

            if(!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoDb = await _repo.GetPhoto(id);
            if(photoDb.IsMain)
                return BadRequest("Already is the main photo");

            var currentMainPhoto = await _repo.GetMainPhotoOfUser(userId);
            currentMainPhoto.IsMain = false;
            photoDb.IsMain = true;

            if(await _repo.SaveAll())
                return NoContent();

            return BadRequest("Error in setting main photo");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId);

            if(!userFromRepo.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoDb = await _repo.GetPhoto(id);
            if(photoDb.IsMain)
                return BadRequest("Main photo cannot be deleted");

            if (photoDb.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoDb.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                {
                    _repo.Delete(photoDb);
                }
            }
            else 
            {
                _repo.Delete(photoDb);
            }

            if(await _repo.SaveAll())
                return Ok();
            
            return BadRequest("Failed to delete");
        }
    }    
}