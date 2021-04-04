using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Services;
using DTO.GET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTO.Models;
using DTO.POST;
using System.Security.Claims;
using System.IO;

namespace BlogApplication_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        //
        private Guid currentUserId;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            //Guid
            ServiceResponse<List<GET_PostDTO>> response = _postService.GetBlogList();

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult MyBlog(Guid id)
        {
            ServiceResponse<List<GET_PostDTO>> response = _postService.GetMyBlog(id);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);

        }

        [HttpGet("{postId}")]
        public IActionResult PostDetail(int postId)
        {
            ServiceResponse<GET_PostDTO> response = _postService.GetSingleBlog(postId);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);

        }

        [HttpPost]
        [Authorize]
        public IActionResult AddAPost([FromForm] PostModel postModel)
        {
            currentUserId = _postService.GetGuid();
            ServiceResponse<PostModel> response = new ServiceResponse<PostModel>();
            if (postModel.FormFile != null && postModel.FormFile != null)
            {
                string uniqueFileName = GetUniqueFileName(postModel.FormFile.FileName);
                //Upload file
                try
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", uniqueFileName);
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        postModel.FormFile.CopyTo(stream);

                    }
                    postModel.PictureUrl = uniqueFileName;
                    response = _postService.AddAPost(postModel, currentUserId);
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            //

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("{postId}")]
        [Authorize]
        public IActionResult EditAPost([FromForm] PostModel postModel, int postId)
        {
            currentUserId = _postService.GetGuid();

            if (postModel.FormFile != null)
            {
                string uniqueFileName = GetUniqueFileName(postModel.FormFile.FileName);
                //Upload file
                try
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", uniqueFileName);
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        postModel.FormFile.CopyTo(stream);

                    }
                    postModel.PictureUrl = uniqueFileName;
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            else
            {
                //get current file name if user didn't upload a new image
                string currentFileName = postModel.PictureUrl;
                int startIndex = currentFileName.IndexOf("/img/") + 5; //file name after /img/
                int endIndex = currentFileName.Length - startIndex;

                string fileName = currentFileName.Substring(startIndex, endIndex);
                postModel.PictureUrl = fileName;
            }
            ServiceResponse<PostModel> response = _postService.EditAPost(postModel, currentUserId, postId);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{postId}")]
        [Authorize]
        public async Task<IActionResult> RemoveAPost(int postId)
        {
            currentUserId = _postService.GetGuid();
            ServiceResponse<PostModel> response = await _postService.RemoveAPost(currentUserId, postId);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //Concate guid to image file name to create unique name
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }
    }
}
