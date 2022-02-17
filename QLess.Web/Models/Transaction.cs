using System.ComponentModel.DataAnnotations.Schema;

namespace QLess.Web.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int TransportCardId { get; set; }
        public string? TransactionType { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public decimal Balance { get; set; }
    }
}
