
using Microsoft.AspNetCore.Identity;

namespace CRMApi.Domain.Models
{
    public class Developer : ApplicationUser
    {   
      public List<string> Stack { get; set; } = new ();
      public List<Team> Teams { get; set; } = new ();       
    }
    
}