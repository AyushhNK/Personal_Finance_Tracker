using Microsoft.AspNetCore.Identity;

namespace FinanceTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Transaction> Transactions { get; set; }
    }
}


