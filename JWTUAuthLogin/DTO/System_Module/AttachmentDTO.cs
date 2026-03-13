using System.Text.Json.Serialization;

namespace JWTUAuthLogin.DTO.System_Module
{
    public class AttachmentDTO
    {
        [JsonPropertyName("ImageData")]
        public string? ImageData { get; set; }

        [JsonPropertyName("RefID")]
        public string? RefID { get; set; }

        [JsonPropertyName("RefType")]
        public string? RefType { get; set; }

        [JsonPropertyName("RefCategory")]
        public string? RefCategory { get; set; }

        [JsonPropertyName("FileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("Active")]
        public string? Active { get; set; }

        [JsonPropertyName("CreatedBy")]
        public string? CreatedBy { get; set; }

        [JsonPropertyName("CreatedOn")]
        public string? CreatedOn { get; set; }

        [JsonPropertyName("ModifiedBy")]
        public string? ModifiedBy { get; set; }

        [JsonPropertyName("ModifiedOn")]
        public string? ModifiedOn { get; set; }
    }
}
