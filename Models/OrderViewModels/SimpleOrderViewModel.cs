using System.Reflection.Metadata.Ecma335;

namespace TestServer.Models.OrderViewModels
{
    public class SimpleOrderViewModel
    {
        public SimpleOrderViewModel()
        {
            Pallets = new List<SimplePalletViewModel>();
        }
        public int Id { get; set; }
        public string BuyerName { get; set; }
        public decimal Price { get; set; }
        public int InvoiceNumber { get; set; }
        public List<SimplePalletViewModel> Pallets { get; set; }
        public int Sum { get; set; }
    }
}
