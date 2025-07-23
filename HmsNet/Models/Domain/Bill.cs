namespace HmsNet.Models.Domain
{
    public class Bill
    {
        public int BillId { get; set; }
        public int OrderId { get; set; }
        public DateTime BillDateTime { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public Order? Order { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
