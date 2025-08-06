using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestServer.Services.OrderService;

namespace TestServer.Controllers
{
    public class OrderController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IOrderService _orderService;
        public OrderController(IWebHostEnvironment env, IOrderService orderService)
        {
            _env = env;
            _orderService = orderService;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Upload()
        {
            return this.View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult UploadExcel(IFormFile excelFile)
        {
            var data = _orderService.GetInformationFromExcel(excelFile);
            if (data == null)
            {
                TempData["Message"] = "Няма прикачен файл!";
                TempData["Danger"] = "True";
                TempData.Keep();
                return RedirectToAction("Upload", "Order");
            }
            if (_orderService.DoesOrderExist(data))
            {
                TempData["Message"] = "Поръчката вече съществува!";
                TempData["Danger"] = "True";
                TempData.Keep();
                return RedirectToAction("Upload", "Order");
            }
            _orderService.AddOrder(data);
            _orderService.AddNewPallets(data.Pallets);
            _orderService.AddProductsToOrder(data);
            TempData["Message"] = "Поръчката качена успешно";
            TempData["Danger"] = "False";
            TempData.Keep();
            return RedirectToAction("Upload", "Order");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Table()
        {
            var data = _orderService.GetSimpleOrders(false);
            ViewBag.FilterStatus = "False";
            if (TempData["OrderFilter"] != null && TempData["OrderFilter"].ToString() == "True")
            {
                ViewBag.FilterStatus = "True";
                data = _orderService.GetSimpleOrders(true);
            }
            TempData.Clear();
            return this.View(data);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CompleteOrder(int orderId)
        {
            _orderService.CompleteOrder(orderId);

            return RedirectToAction("Table");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult OrderLoad(string filter)
        {
            if (filter == "Completed")
            {
                TempData["OrderFilter"] = "True";
            }
            return this.RedirectToAction("Table", "Order");
        }
    }
}
