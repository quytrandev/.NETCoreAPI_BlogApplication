using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DALayer.Entities
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string PictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<PostComment> PostComments { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
