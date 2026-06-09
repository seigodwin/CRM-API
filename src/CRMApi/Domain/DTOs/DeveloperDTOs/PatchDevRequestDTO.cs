using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs.DeveloperDTOs
{
    public class PatchDevRequestDTO
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [DataType(DataType.PhoneNumber)]
        public List<string> Stack { get; set; } = new List<string>();
    }
}
