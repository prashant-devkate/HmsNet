namespace HmsNet.Models.Domain
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int BillId { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionStatus { get; set; }
        public Bill? bill { get; set; }
    }
}
