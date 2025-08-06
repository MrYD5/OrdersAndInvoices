using TestServer.Data;

namespace TestServer.Models.OrderViewModels
{
    public class PalletViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Pallet Pallet { get; set; }
        public double Quantity { get; set; }
        public int PalletNumber { get; set; }
    }
}
