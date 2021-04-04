using System;
using System.ComponentModel.DataAnnotations;

namespace DTO.GET
{
    public class GET_PostDTO
    {
        [Required]
        public int PostId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string PictureUrl { get; set; }
        public string Username { get; set; }
        public int TotalComment { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
