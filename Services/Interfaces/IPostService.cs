using DTO.GET;
using DTO.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public interface IPostService
    {
        Guid GetGuid();
        ServiceResponse<List<GET_PostDTO>> GetBlogList();
        ServiceResponse<List<GET_PostDTO>> GetMyBlog(Guid userId);
        ServiceResponse<GET_PostDTO> GetSingleBlog(int postId);
        ServiceResponse<PostModel> AddAPost(PostModel postModel, Guid userId);
        ServiceResponse<PostModel> EditAPost(PostModel postModel, Guid userId, int postId);
        Task<ServiceResponse<PostModel>> RemoveAPost(Guid userId, int postId);


    }
}