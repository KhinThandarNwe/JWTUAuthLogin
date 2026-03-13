using System.Text.Json.Serialization;

namespace JWTUAuthLogin.DTO.Login_Module
{
    public class UserDataDTO
    {
        [JsonPropertyName("Username")]
        public string? UserName { get; set; }

        [JsonPropertyName("Email")]
        public string? Email { get; set; }

        [JsonPropertyName("Password")]
        public string? Password { get; set; }

        [JsonPropertyName("EmailConfirmation")]
        public string? EmailConfirmation { get; set; }

        [JsonPropertyName("OS")]
        public string? Os { get; set; }

        [JsonPropertyName("OSVersion")]
        public string? OSVersion { get; set; }

        [JsonPropertyName("ModelNo")]
        public string? ModelNo { get; set; }

        [JsonPropertyName("Manufacturer")]
        public string? Manufacturer { get; set; }

        [JsonPropertyName("RegToken")]
        public string? RegToken { get; set; }

        [JsonPropertyName("LocalTimeZone")]
        public string? LocalTimeZone { get; set; }

        [JsonPropertyName("DeviceID")]
        public string? DeviceID { get; set; }
    }
}
