using AutoMapper;
using DALayer;
using DALayer.Entities;
using DTO.GET;
using DTO.Models;
using DTO.POST;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Services
{
    public class PostService : IPostService
    {
        BlogApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public PostService(BlogApplicationDbContext context,  IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;

        }
        //Read

        public ServiceResponse<List<GET_PostDTO>> GetBlogList()
        {
            ServiceResponse<List<GET_PostDTO>> response = new ServiceResponse<List<GET_PostDTO>>();

            var data = (from post in _context.Posts
                        join user in _context.Users on post.UserId equals user.UserId
                        select new GET_PostDTO
                        {
                            PostId = post.PostId,
                            Title = post.Title,
                            Content = post.Content,
                            PictureUrl = post.PictureUrl,
                            Username = user.Username,
                            CreatedAt = post.CreatedAt,
                            TotalComment = 0,
                            //TotalComment = TotalComment(post.PostId) memory leak warning
                        }).OrderByDescending(post => post.CreatedAt).ToList();

            List<GET_PostDTO> listPost = new List<GET_PostDTO>();
            for (int i = 0; i< data.Count(); i++)
            {
                data[i].PictureUrl = GetPictureUrl(data[i].PictureUrl);
                data[i].TotalComment = TotalComment(data[i].PostId);
                listPost.Add(data[i]);
            }
            
            if (listPost.Count > 0)
            {
                response.Data = listPost;
                response.isSuccess = true;
                response.Message = "Posts have been loaded successfully";
            }
            else
            {
                response.isSuccess = false;
                response.Message = "Failed to load posts";
            }

            return response;
        }

        public ServiceResponse<List<GET_PostDTO>> GetMyBlog(Guid userId)
        {
            ServiceResponse<List<GET_PostDTO>> response = new ServiceResponse<List<GET_PostDTO>>();

            var data = (from post in _context.Posts
                        join user in _context.Users on post.UserId equals user.UserId
                        where post.UserId.Equals(userId)
                        select new GET_PostDTO
                        {
                            PostId = post.PostId,
                            Title = post.Title,
                            Content = post.Content,
                            PictureUrl = post.PictureUrl,
                            Username = user.Username,
                            CreatedAt = post.CreatedAt,
                            TotalComment = 0,
                            //TotalComment = TotalComment(post.PostId) memory leak warning
                        }).ToList();

            List<GET_PostDTO> listPost = new List<GET_PostDTO>();
            for (int i = 0; i < data.Count(); i++)
            {
                data[i].PictureUrl = GetPictureUrl(data[i].PictureUrl);
                data[i].TotalComment = TotalComment(data[i].PostId);
                listPost.Add(data[i]);
            }

            if (listPost.Count > 0)
            {
                response.Data = listPost;
                response.isSuccess = true;
                response.Message = "Posts have been loaded successfully";
            }
            else
            {
                response.isSuccess = false;
                response.Message = "Failed to load posts";
            }

            return response;
        }

        public ServiceResponse<GET_PostDTO> GetSingleBlog(int postId)
        {
            ServiceResponse<GET_PostDTO> response = new ServiceResponse<GET_PostDTO>();

            var data = (from post in _context.Posts
                        join user in _context.Users on post.UserId equals user.UserId
                        where post.PostId == postId
                        select new GET_PostDTO
                        {
                            PostId = post.PostId,
                            Title = post.Title,
                            Content = post.Content,
                            PictureUrl = post.PictureUrl,
                            Username = user.Username,
                            CreatedAt = post.CreatedAt,
                            UserId = post.UserId,                         
                            //TotalComment = _commentService.GetCommentList(blog.BlogId).Count()  //memory leak warning
                        }).FirstOrDefault();

            if (data != null)
            {
                data.PictureUrl = GetPictureUrl(data.PictureUrl);
                data.TotalComment = TotalComment(data.PostId);
                response.Data = data;
                response.isSuccess = true;
                response.Message = "Post have been loaded successfully";
            }
            else
            {
                response.isSuccess = false;
                response.Message = "Failed to get the blog with id: " + postId;
            }
            return response;
        }

        //
        public Guid GetGuid()
        {
            Guid guid = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return guid;
        }
        //CRUD

        [Authorize]
        public ServiceResponse<PostModel> AddAPost(PostModel postModel, Guid userId)
        {
            ServiceResponse<PostModel> response = new ServiceResponse<PostModel>();

            if (postModel != null )
            {
                POST_PostDTO postDTO = new POST_PostDTO()
                {
                    Title = postModel.Title,
                    Content = postModel.Content,
                    PictureUrl = postModel.PictureUrl,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UserId = userId
                };

                Post postMapper = _mapper.Map<Post>(postDTO);
                _context.Posts.Add(postMapper);
                _context.SaveChanges();

                response.currentItemId = postMapper.PostId;
                response.isSuccess = true;
                response.Message = "Post is successfully created";
            }
            else
            {
                response.isSuccess = false;
                response.Message = "Failed to create a post";
            }
            return response;
        }

        public ServiceResponse<PostModel> EditAPost(PostModel postModel, Guid userId, int postId)
        {
            ServiceResponse<PostModel> response = new ServiceResponse<PostModel>();

            if (postModel != null && userId != null)
            {
                Post post = _context.Posts.Where(p => p.PostId.Equals(postId)).FirstOrDefault();
                if (post.UserId.Equals(userId))
                {
                    post.Title = postModel.Title.Trim();
                    post.Content = postModel.Content.Trim();
                    post.PictureUrl = postModel.PictureUrl;
                    post.UpdatedAt = DateTime.Now;

                    _context.Posts.Update(post);
                    _context.SaveChanges();

                    response.currentItemId = post.PostId;
                    response.isSuccess = true;
                    response.Message = "Post is successfully updated";
                }
                else
                {
                    response.currentItemId = post.PostId;
                    response.isSuccess = false;
                    response.Message = "Failed to update a post";
                }
            }
            return response;
        }

        public async Task<ServiceResponse<PostModel>> RemoveAPost(Guid userId, int postId)
        {
            ServiceResponse<PostModel> response = new ServiceResponse<PostModel>();

            if (userId != null)
            {
                Post post = _context.Posts.Where(p => p.PostId.Equals(postId)).FirstOrDefault();
                if (post.UserId.Equals(userId))
                {
                    //Cascade removing
                    //Load list of comment belongs to a post which is about to be removed
                    IList<Comment> commentsToBeRemoved = (from comment in _context.Comments
                                                          join postComment in _context.PostComments on comment.CommentId equals postComment.CommentId
                                                          where postComment.PostId.Equals(postId)
                                                          select comment).ToList();
      
                    //Remove a post
                    _context.Posts.Remove(post);
                    await _context.SaveChangesAsync();

                    //Remove all the comments belong to the post
                    _context.Comments.RemoveRange(commentsToBeRemoved);
                    await _context.SaveChangesAsync();

                    response.isSuccess = true;
                    response.Message = "Post is successfully removed";
                }
                else
                {
                    response.isSuccess = false;
                    response.Message = "Failed to remove a post";
                }
            }
            return response;
        }

        public int TotalComment(int postId)
        {
            var data = (from postComment in _context.PostComments
                        join comment in _context.Comments on postComment.CommentId equals comment.CommentId
                        join post in _context.Posts on postComment.PostId equals post.PostId
                        where postComment.PostId == postId
                        select new
                        {
                            CommentId = comment.CommentId,
                            Content = comment.Content,
                        }).ToList();
            int totalComment = data.Count();
            return totalComment;
        }
        public string GetPictureUrl(string pictureName)
        {
            string url = String.Format("{0}://{1}{2}/wwwroot/img/{3}",
                             _httpContextAccessor.HttpContext.Request.Scheme,
                             _httpContextAccessor.HttpContext.Request.Host,
                             _httpContextAccessor.HttpContext.Request.PathBase, pictureName);
            return url;
        }
    }
}
