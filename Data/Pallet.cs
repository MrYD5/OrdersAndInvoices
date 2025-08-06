namespace TestServer.Data
{
    public class Pallet
    {
        public Pallet()
        {
            this.Orders = new HashSet<OrderPallet>();
        }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Dimensions { get; set; }
        public ICollection<OrderPallet> Orders { get; set; }
    }
}
