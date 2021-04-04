using System;
using System.ComponentModel.DataAnnotations;

namespace DTO.POST
{
    public class POST_PostDTO
    {
       [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string PictureUrl { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UserId { get; set; }
    }

}
