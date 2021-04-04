using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public interface ICommentService
    {
        ServiceResponse<List<CommentDTO>> GetCommentList(int postId);
        public ServiceResponse<string> AddAComment(string content, int postId, Guid userId);
        Task<ServiceResponse<string>> RemoveAComment(Guid userId, int commentId);
    }
}