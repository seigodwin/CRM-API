using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs.DeveloperDTOs
{
    public class PatchDevRequestDTO
    {
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? UserName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
        public List<String>? Stack { get; set; }
    }
}
