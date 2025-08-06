using Microsoft.Identity.Client;
using TestServer.Data;
using TestServer.Models.OrderViewModels;

namespace TestServer.Services.OrderService
{
    public interface IOrderService
    {
        public OrderViewModel GetInformationFromExcel(IFormFile file);
        public void AddNewPallets(List<PalletViewModel> pallets);
        public void AddBuyer(string name);
        public void AddOrder(OrderViewModel data);
        public void AddProductsToOrder(OrderViewModel data);
        public List<SimpleOrderViewModel> GetSimpleOrders(bool isFulfilled);
        public bool EditableOrder(OrderViewModel order);
        public void EditOrder(OrderViewModel order);
        public bool DoesOrderExist(OrderViewModel order);
        public void CompleteOrder(int orderId);
    }
}
