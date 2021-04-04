using Microsoft.AspNetCore.Http;

namespace DTO.Models
{
    public class PostModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string PictureUrl { get; set; }
        public IFormFile FormFile { get; set; }
    }
}
