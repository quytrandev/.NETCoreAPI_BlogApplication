using System;

namespace DALayer.Entities
{
    public class PostComment
    {
        public int CommentId { get; set; }
        public  Comment Comment { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
        public Guid UserId { get; set; }                
    }
}
