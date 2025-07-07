using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FinanceTracker.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        public int CategoryId { get; set; }
        [ValidateNever]
        public TransactionCategory Category { get; set; }

        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser User { get; set; }
    }

}
