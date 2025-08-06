using TestServer.Data;

namespace TestServer.Models.OrderViewModels
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            Pallets = new List<PalletViewModel>();
        }
        public int Id { get; set; }
        public Buyer Buyer { get; set; }
        public decimal Price { get; set; }
        public List<PalletViewModel> Pallets { get; set; }
        public DateTime Date { get; set; }
        public int OrderNumber { get; set; }
        public bool IsFulfilled { get; set; }

    }
}
