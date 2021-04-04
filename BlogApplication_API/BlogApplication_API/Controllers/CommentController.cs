using DTO;
using DTO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogApplication_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommentController(ICommentService commentService, IHttpContextAccessor httpContextAccessor)
        {
            _commentService = commentService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{postId}")]
        public IActionResult GetCommentList(int postId)
        {
            ServiceResponse<List<CommentDTO>> response = _commentService.GetCommentList(postId);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("{postId}")]
        [Authorize]
        public IActionResult AddAComment([FromBody] string content, int postId)
        {
            Guid currentUserId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ServiceResponse<string> response = _commentService.AddAComment(content, postId, currentUserId);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<IActionResult> RemoveAComment(int commentId)
        {
            Guid currentUserId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ServiceResponse<string> response = await _commentService.RemoveAComment(currentUserId, commentId);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
