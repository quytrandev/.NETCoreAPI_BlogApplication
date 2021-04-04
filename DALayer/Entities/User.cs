
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace DALayer.Entities
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [MaxLength(255)]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }   
        public string AccessToken { get; set; }    
        public string RefreshToken { get; set; }
        public bool ConfirmEmail { get; set; }        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Post> Posts { get; set; }
    }
}
