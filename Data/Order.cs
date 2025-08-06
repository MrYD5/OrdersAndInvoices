namespace TestServer.Data
{
    public class Order
    {
        public Order()
        {
            this.Pallets = new HashSet<OrderPallet>();
        }
        public int Id { get; set; }
        public Buyer Buyer { get; set; }
        public ICollection<OrderPallet> Pallets { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public int InvoiceNumber { get; set; }
        public bool IsFufiled { get; set; }
    }
}
