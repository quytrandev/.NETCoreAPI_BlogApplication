using AutoMapper;
using DALayer;
using DALayer.Entities;
using DTO;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Services
{
    public class UserService : IUserService
    {
        BlogApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(BlogApplicationDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
    
        public ServiceResponse<UserDTO> Register(UserDTO userDTO)
        {
            ServiceResponse<UserDTO> response = new ServiceResponse<UserDTO>();

            if(userDTO != null)
            {
                User userMapper = _mapper.Map<User>(userDTO);
                if(!IsAnExistingUser(userMapper.Email))
                {
                    userMapper.CreatedAt = DateTime.Now;
                    _context.Users.Add(userMapper);
                    _context.SaveChanges();

                    response.isSuccess = true;
                    response.Message = "Successfully registered";
                }
                else
                {
                    response.isSuccess = false;
                    response.Message = "Email is already in use";
                }
            }
            return response;
        }

        public bool IsUserTokenAdded(LoginResult loginResult)
        {
            User user = _context.Users.Where(e => e.Email.Equals(loginResult.Email)).FirstOrDefault();
            if(user != null && loginResult.AccessToken != null && loginResult.RefreshToken != null)
            {
                user.AccessToken = loginResult.AccessToken;
                user.RefreshToken = loginResult.RefreshToken;

                _context.Users.Update(user);
                _context.SaveChanges();

                return true;
            }
            return false;
        }

        //Profile
        public ServiceResponse<UserDTO> GetProfile(Guid userId)
        {
            ServiceResponse<UserDTO> response = new ServiceResponse<UserDTO>();

            if (userId != null)
            {
                bool isUserValid = IsUserExistAndAuthorized(userId);

                if (isUserValid)
                {
                    var data = (from user in _context.Users
                                where user.UserId.Equals(userId)
                                select new UserDTO
                                {
                                    UserId = user.UserId,
                                    Email = user.Email,
                                    Username = user.Username
                                }).FirstOrDefault();
                    response.Data = data;
                    response.isSuccess = true;
                    response.Message = "Authorized user's info loaded";
                }
                else
                {
                    response.isSuccess = false;
                    response.Message = "Failed to load user info";
                }
            }
            else
            {
                response.isSuccess = false;
                response.Message = "User id is empty";
            }
            return response;
        }

        public ServiceResponse<UserDTO> UpdateUsername(Guid userId, string username)
        {
            ServiceResponse<UserDTO> response = new ServiceResponse<UserDTO>();
            if (userId != null && username.Length > 0)
            {
                bool isUserValid = IsUserExistAndAuthorized(userId);

                if (isUserValid)
                {
                    User user = _context.Users.Where(u => u.UserId.Equals(userId)).FirstOrDefault();
                    user.Username = username.Trim();
                    user.UpdatedAt = DateTime.Now;
                    _context.Users.Update(user);
                    _context.SaveChanges();

                    response.isSuccess = true;
                    response.Message = "Username updated";
                }
                else
                {
                    response.isSuccess = false;
                    response.Message = "Failed to update username";
                }
            }
            else
            {
                response.isSuccess = false;
                response.Message = "User id is empty";
            }
            return response;
        }

        public ServiceResponse<UserDTO> ChangePassword(Guid userId, string currentPassword, string newPassword)
        {
            ServiceResponse<UserDTO> response = new ServiceResponse<UserDTO>();

            if (userId != null && currentPassword.Length > 0 && newPassword.Length > 0)
            {
                bool isUserValid = IsUserExistAndAuthorized(userId);
                if (isUserValid)
                {
                    User user = _context.Users.Where(u => u.UserId.Equals(userId)).FirstOrDefault();
                    var currentHashedPassword = user.Password;

                    bool isPasswordValid = false;
                    if (currentHashedPassword.Length > 0)
                    {                      
                        isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(currentPassword.Trim(), currentHashedPassword.Trim(),BCrypt.Net.HashType.SHA256);              
                    }

                    if(isPasswordValid)
                    {
                        if(newPassword.Length > 0)
                        {
                            if(!currentPassword.Equals(newPassword))
                            {
                                user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword.Trim(), BCrypt.Net.HashType.SHA256);
                                user.UpdatedAt = DateTime.Now;
                                _context.Users.Update(user);
                                _context.SaveChanges();

                                response.isSuccess = true;
                                response.Message = "Password has been changed";
                            }
                            else
                            {
                                response.isSuccess = false;
                                response.Message = "Current and new password must be different";
                            }
                        }
                        else
                        {
                            response.isSuccess = false;
                            response.Message = "New password cannot be empty";
                        }                      
                    }
                    else
                    {
                        response.isSuccess = false;
                        response.Message = "Current password is incorrect";
                    }
                }
                else
                {
                    response.isSuccess = false;
                    response.Message = "Failed to change password";
                }
            }
            else
            {
                response.isSuccess = false;
                response.Message = "User id is empty";
            }
            return response;
        }
        //Check if user exists and the current is user is the correct one
        public bool IsUserExistAndAuthorized(Guid userId)
        {
            bool isExist = false;
            Guid currentUserId = GetGuid();
            if (userId != null && userId.Equals(currentUserId))
            {
                User user = _context.Users.Where(u => u.UserId.Equals(userId)).FirstOrDefault();
                if (user != null)
                {
                    isExist = true;
                }
                else
                {
                    isExist = false;
                }
            }
            return isExist;
        }
        public Guid GetGuid()
        {
            Guid guid = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return guid;
        }
        //check user's credentials
        public bool IsValidUserCredentials(LoginRequest loginRequest)
        {
            bool isValid;
            if (string.IsNullOrWhiteSpace(loginRequest.Email))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return false;
            }

            try
            {
                var currentHashedPassword = _context.Users.Where(email => email.Email.Equals(loginRequest.Email)).FirstOrDefault().Password;
                //check if request email exists
                if (currentHashedPassword.Length > 0)
                {
                    isValid = BCrypt.Net.BCrypt.EnhancedVerify(loginRequest.Password.Trim(), currentHashedPassword.Trim(), BCrypt.Net.HashType.SHA256);
                    return isValid;
                }
                else
                {
                    isValid = false;
                    return isValid;
                }

            }
            catch
            {
                isValid = false;
                return isValid;
            }

        }

        public bool IsAnExistingUser(string email)
        {
            bool isExist = false;
            if(email != null)
            {
                User user = _context.Users.Where(e => e.Email.Equals(email)).FirstOrDefault();
                if (user != null)
                {
                    isExist = true;
                }
                else
                {
                    isExist = false;
                }
            }
            return isExist;
        }
        public string GetUserId(string email)
        {
            var userId = _context.Users.Where(e => e.Email.Equals(email)).FirstOrDefault().UserId.ToString();

            return userId;
        }
        
        
    }
}
