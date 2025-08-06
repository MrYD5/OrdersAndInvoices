namespace TestServer.Models.HomeViewModels
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }

        public string ProductName { get; set; } = null!;

        public double Amount { get; set; }

        public decimal SinglePrice { get; set; }

        public decimal SumNoTax { get; set; }

        public decimal SumToPay { get; set; }

        public string BuyerName { get; set; } = null!;

        public string SellerName { get; set; } = null!;

        public string InvoiceNumber { get; set; } = null!;

        public DateOnly Date { get; set; }
    }
}
