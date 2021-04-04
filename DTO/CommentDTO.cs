using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class CommentDTO
    {
        [Required]
        public int CommentId { get; set; }
        [Required]
        public string Content { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CommentOwnerId { get; set; }
    }
}
