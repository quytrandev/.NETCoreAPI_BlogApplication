using AutoMapper;
using DALayer.Entities;
using DTO;
using DTO.GET;
using DTO.Models;
using DTO.POST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApplication_API.AutoMapperProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<GET_PostDTO, Post>();
            //CreateMap<PostModel, POST_PostDTO>();
            CreateMap<POST_PostDTO, Post>();
            CreateMap<UserDTO, User>();
        }
    }
}
