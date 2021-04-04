using Microsoft.AspNetCore.Http;


namespace DTO.Models
{
    public class ImageDTO
    {
        public string FileName { get; set; }
        public IFormFile FormFile { get; set; }

    }
}
