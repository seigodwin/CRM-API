
using System.ComponentModel.DataAnnotations.Schema;
using CRMApi.Domain.Models;

namespace CRM_API.Domain.Models
{
    public class RefreshToken
    {
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public DateTime AddedDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; } 
    }
}