using CRMApi.Domain.Models;

namespace CRMApi.Domain.DTOs
{
    public class DevRegistrationDTO
    {

        public IFormFile? Image { get; set; }
        public required string Name { get; set; }
        public required string Contact { get; set; }
        public required string Email { get; set; }
        public List<string>? Stack { get; set; }

    }
}
