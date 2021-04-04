using DALayer;
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using DALayer.Entities;
using System.Threading.Tasks;

namespace Services
{
    public class CommentService : ICommentService
    {
        BlogApplicationDbContext _context;


        public CommentService(BlogApplicationDbContext context)
        {
            _context = context;
        }

        public ServiceResponse<List<CommentDTO>> GetCommentList(int postId)
        {
            ServiceResponse<List<CommentDTO>> response = new ServiceResponse<List<CommentDTO>>();

            var data = (from postComment in _context.PostComments
                        join comment in _context.Comments on postComment.CommentId equals comment.CommentId
                        join post in _context.Posts on postComment.PostId equals post.PostId
                        where postComment.PostId == postId
                        select new CommentDTO
                        {
                            CommentOwnerId = postComment.UserId,
                            CommentId = comment.CommentId,
                            Content = comment.Content,
                            CreatedAt = comment.CreatedAt
                            //Username = getUsername(guid) will cause memory leak warning
                        }).ToList();

            List<CommentDTO> listComment = new List<CommentDTO>();
            for (int i = 0; i < data.Count(); i++)
            {
                //prevent memory leak
                data[i].Username = getUsername(data[i].CommentOwnerId);
                listComment.Add(data[i]);
            }
            if (listComment.Count > 0)
            {
                response.Data = listComment;
                response.isSuccess = true;
                response.Message = "Comments have been loaded successfully";
            }
            else
            {
                response.Data = null;
                response.isSuccess = true;
                response.Message = "This post has 0 comment(s)";
            }

            return response;
        }

        public ServiceResponse<string> AddAComment(string content, int postId, Guid userId)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            bool isExisted = true;
            //check if post still exists
            var currentPost = (from post in _context.Posts
                           where post.PostId.Equals(postId)
                           select new
                           {
                               PostContent = post.Content
                           }
                           ).FirstOrDefault();
                       
            if(currentPost == null)
            {
                isExisted = false;
            }
            if (!string.IsNullOrEmpty(content) && userId != null && isExisted == true)
            {
                Comment comment = new Comment()
                {
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                };
                _context.Comments.Add(comment);
                _context.SaveChanges();

                PostComment postComment = new PostComment()
                {
                    CommentId = comment.CommentId,
                    PostId = postId,
                    UserId = userId
                };
                _context.PostComments.Add(postComment);
                _context.SaveChanges();

                response.Data = comment.Content;
                response.isSuccess = true;
                response.currentItemId = postId;
                response.Message = "Comment is successfully created";
            }
            else
            {
                response.isSuccess = false;
                response.Message = "Failed to create a comment";
            }
            return response;
        }

        public async Task<ServiceResponse<string>> RemoveAComment(Guid userId, int commentId)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            if (userId != null)
            {
                //Verify if the comment belongs to the current user
                PostComment postComment = _context.PostComments.Where(c => c.CommentId.Equals(commentId))
                    .Where(u => u.UserId.Equals(userId)).FirstOrDefault();

                if (postComment != null)
                {
                    Comment commentToBeRemoved = _context.Comments.Where(c => c.CommentId.Equals(commentId)).FirstOrDefault();
                    _context.Comments.Remove(commentToBeRemoved);
                    await _context.SaveChangesAsync();

                    response.isSuccess = true;
                    response.Message = "Comment is successfully removed";
                }
                else
                {
                    response.isSuccess = false;
                    response.Message = "Failed to remove a comment";
                }
            }
            return response;
        }

        public string getUsername(Guid userId)
        {
            string username = _context.Users.Where(u => u.UserId.Equals(userId)).FirstOrDefault().Username;
            return username;
        }
    }
}
