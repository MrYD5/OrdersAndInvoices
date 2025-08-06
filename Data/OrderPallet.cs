namespace TestServer.Data
{
    public class OrderPallet
    {
        public int PalletId { get; set; }
        public Pallet Pallet { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public double Quantity { get; set; }
        public int PalletsNumber { get; set; }
    }
}
