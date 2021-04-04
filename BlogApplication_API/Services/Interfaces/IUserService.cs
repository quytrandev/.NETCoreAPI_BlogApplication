using DTO;
using System;

namespace Services
{
    public interface IUserService
    {
        bool IsAnExistingUser(string userName);
        bool IsValidUserCredentials(LoginRequest loginRequest);
        ServiceResponse<UserDTO> Register(UserDTO userDTO);
        string GetUserId(string email);
        bool IsUserTokenAdded(LoginResult loginResult);
        ServiceResponse<UserDTO> GetProfile(Guid userId);
        Guid GetGuid();
        ServiceResponse<UserDTO> UpdateUsername(Guid userId, string username);
        ServiceResponse<UserDTO> ChangePassword(Guid userId, string currentPassword, string newPassword);
    }
}