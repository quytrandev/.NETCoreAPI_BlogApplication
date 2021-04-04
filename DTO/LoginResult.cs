using System;
using System.Text.Json.Serialization;

namespace DTO
{
    public class LoginResult
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
