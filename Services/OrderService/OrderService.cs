using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using System.Xml.Linq;
using TestServer.Data;
using TestServer.Models.OrderViewModels;

namespace TestServer.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly GoceGidContext db;
        public OrderService(GoceGidContext db)
        {
            this.db = db;
        }

        public void AddBuyer(string name)
        {
            if (!db.Buyers.Any(x => x.Name.ToLower() == name.ToLower()))
            {
                db.Buyers.Add(new Buyer
                {
                    Name = name,
                });
                db.SaveChanges();
            }
        }

        public void AddNewPallets(List<PalletViewModel> pallets)
        {
            foreach (var pallet in pallets)
            {
                pallet.Pallet.Code = ConvertCyrillicToLatin(pallet.Pallet.Code);
                if (!db.Pallets.Any(x =>
                            x.Name == pallet.Pallet.Name &&
                            x.UnitPrice == pallet.Pallet.UnitPrice &&
                            x.Code == pallet.Pallet.Code &&
                            x.Color == pallet.Pallet.Color &&
                            ((x.Dimensions == null && pallet.Pallet.Dimensions == null) ||
                            (x.Dimensions != null && x.Dimensions == pallet.Pallet.Dimensions))))
                {
                    db.Pallets.Add(pallet.Pallet);
                    db.SaveChanges();
                }
            }
        }

        public void AddOrder(OrderViewModel data)
        {
            if (!DoesOrderExist(data))
            {
                if (EditableOrder(data))
                {
                    EditOrder(data);
                }
                db.Orders.Add(new Order()
                {
                    Buyer = data.Buyer,
                    Price = data.Price,
                    Date = data.Date,
                    IsFufiled = false,
                    InvoiceNumber = data.OrderNumber,
                });
                db.SaveChanges();
            }
        }

        public void AddProductsToOrder(OrderViewModel data)
        {
           var order = db.Orders.Where(x => x.IsFufiled == false && x.Buyer == data.Buyer && x.Price == data.Price && x.Date == data.Date && x.InvoiceNumber == data.OrderNumber).FirstOrDefault();
            if (!db.OrderPallets.Any(x => x.OrderId == order.Id))
            {
                foreach (var pallet in data.Pallets)
                {
                    var realPallet = db.Pallets.Where(x =>
                        x.Name == pallet.Pallet.Name &&
                        x.UnitPrice == pallet.Pallet.UnitPrice &&
                        x.Code == pallet.Pallet.Code &&
                        x.Color == pallet.Pallet.Color &&
                        x.Dimensions == pallet.Pallet.Dimensions).FirstOrDefault();

                    var orderPallet = new OrderPallet()
                    {
                        OrderId = order.Id,
                        Order = order,
                        PalletId = realPallet.Id,
                        Pallet = realPallet,
                        PalletsNumber = pallet.PalletNumber,
                        Quantity = pallet.Quantity,
                    };
                    db.OrderPallets.Add(orderPallet);
                    db.SaveChanges();
                }
            }
        }

        public void CompleteOrder(int orderId)
        {
            var order = db.Orders.Where(x => x.Id == orderId).FirstOrDefault();
            order.IsFufiled = true;
            db.SaveChanges();
        }

        public bool DoesOrderExist(OrderViewModel data)
        {
            if (db.Orders.Any(x => x.InvoiceNumber == data.OrderNumber &&
            x.IsFufiled == false && x.Buyer == data.Buyer && x.Price == data.Price && x.Date == data.Date))
            {
                return true;
            }
            return false;
        }

        public bool EditableOrder(OrderViewModel order)
        {
            if (db.Orders.Any(x => x.InvoiceNumber == order.OrderNumber && x.Buyer.Id == order.Buyer.Id && x.Date != order.Date))
            {
                return true;
            }
            return false;
        }

        public void EditOrder(OrderViewModel order)
        {
            var oldOrder = db.Orders.Where(x => x.InvoiceNumber == order.OrderNumber && x.Buyer.Name == order.Buyer.Name).FirstOrDefault();
            db.Orders.Remove(oldOrder);
            db.SaveChanges();
        }

        public OrderViewModel GetInformationFromExcel(IFormFile data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }
            using var stream = data.OpenReadStream();
            using var file = new XLWorkbook(stream);
            var sheet = file.Worksheet("Sheet2");
            IXLAddress productStart = null;
            IXLAddress productEnd = null;
            foreach (var cell in sheet.Column(sheet.FirstColumnUsed().ColumnNumber()).CellsUsed())
            {
                if (cell.Value.ToString().Trim() == "numbers")
                {
                    productStart = cell.Address;
                }
            }
            for (int i = productStart.RowNumber; i < 100; i++)
            {
                if (sheet.Column(productStart.ColumnLetter).Cell(i).Value.ToString() == "")
                {
                    productEnd = sheet.Column(sheet.LastColumnUsed().ColumnNumber()).Cell(i - 1).Address;
                    break;
                }
            }

            var range = sheet.Range(productStart, productEnd);
            range = range.Range(2, 2, range.RowCount(), range.ColumnCount());
            
            OrderViewModel viewModel = new OrderViewModel();

            foreach (var row in range.Rows())
            {
                Console.WriteLine(row.LastCellUsed().Address.ColumnNumber);
              PalletViewModel pallet = new PalletViewModel()
              {
                  Pallet = new Pallet()
                  {
                  Code = row.Cell(1).Value.ToString(),
                  Dimensions = row.Cell(2) == null ? null : row.Cell(2).Value.ToString(),
                  Color = row.Cell(3).Value.ToString().Split(" ").ToArray()[0],
                  Name = row.Cell(3).Value.ToString(),
                  UnitPrice = decimal.Parse(row.Cell(row.LastCellUsed().Address.ColumnNumber - 3).Value.ToString()),
                  },
                  Quantity = row.Cell(row.LastCellUsed().Address.ColumnNumber - 5).Value.GetNumber(),
                  PalletNumber = int.Parse(row.Cell(row.LastCellUsed().Address.ColumnNumber - 7).Value.ToString()),
              };
               viewModel.Pallets.Add(pallet);
            }


            foreach (var colum in sheet.Columns())
            {
                foreach (var cell in colum.CellsUsed())
                {
                    if (cell.Value.ToString() == "consignee:")
                    {
                        var name = sheet.Column(colum.ColumnNumber() + 1).Cell(cell.Address.RowNumber).Value.ToString();
                        if (name == string.Empty)
                        {
                            name = sheet.Column(colum.ColumnNumber() + 1).Cell(cell.Address.RowNumber + 1).Value.ToString();
                        }
                        AddBuyer(name);

                        viewModel.Buyer = db.Buyers.Where(x => x.Name == name).FirstOrDefault();
                    }
                }
            }
            var lastRow = sheet.LastRowUsed().RowNumber();
            for (int i = 1; i < 100; i++)
            {
                if (sheet.Column(i).Cell(lastRow).Value.ToString() == "DATE:")
                {
                    viewModel.Date = DateTime.Parse(sheet.Column(i + 1).Cell(lastRow).Value.ToString());
                }
            }
            viewModel.Price = decimal.Parse(sheet.Column(sheet.LastColumnUsed().ColumnNumber()).Cell(productEnd.RowNumber + 4).Value.ToString());

            var invoiceString = sheet.FirstRow().FirstCellUsed().Value.ToString().Split(" ").ToArray();
            var invoice = invoiceString[invoiceString.Length - 1];
            var startIndex = 0;
            for (int i = 0; i < invoice.Length - 1; i++)
            {
                if (invoice[i].ToString() == "0")
                {
                    startIndex++;
                    continue;
                }
                break;
            }
            viewModel.OrderNumber = int.Parse(invoice.Substring(startIndex, invoice.Length - startIndex));
            return viewModel;
        }

        public List<SimpleOrderViewModel> GetSimpleOrders(bool isFulfilled)
        {
            var orders = db.Orders.Where(x => x.IsFufiled == isFulfilled).Select(x => new SimpleOrderViewModel()
            {
               Id = x.Id,
               BuyerName = x.Buyer.Name,
               InvoiceNumber = x.InvoiceNumber,
               Price = x.Price,
            }).ToList();
            foreach (var order in orders)
            {
                var pallets = db.OrderPallets.Where(x => x.OrderId == order.Id).Select(x => new SimplePalletViewModel()
                {
                    Name = x.Pallet.Code.ToString().Replace(" ", "") + " " + x.Pallet.Dimensions.ToString(),
                    PalletNumber = x.PalletsNumber,
                }).ToList();
                int sum = 0;
                foreach (var palletNumber in pallets)
                {
                    sum += palletNumber.PalletNumber; 
                }
                order.Pallets = pallets;
                order.Sum = sum;
            }
            return orders;
        }
        private static string ConvertCyrillicToLatin(string input)
        {
            // Примерна карта за основните визуално сходни символи
            Dictionary<char, char> map = new Dictionary<char, char>
    {
        { 'А', 'A' }, { 'В', 'B' }, { 'Е', 'E' }, { 'К', 'K' }, { 'М', 'M' },
        { 'Н', 'H' }, { 'О', 'O' }, { 'Р', 'P' }, { 'С', 'C' }, { 'Т', 'T' },
        { 'Х', 'X' }, { 'а', 'a' }, { 'е', 'e' }, { 'о', 'o' }, { 'р', 'p' },
        { 'с', 'c' }, { 'х', 'x' }, { 'к', 'k' }, { 'м', 'm' }, { 'в', 'b' },
        { 'н', 'h' }, { 'т', 'm' }
    };

            var result = new StringBuilder();

            foreach (char ch in input)
            {
                if (map.ContainsKey(ch))
                    result.Append(map[ch]);
                else
                    result.Append(ch); // Запази оригинала ако не е кирилица или няма съответствие
            }

            return result.ToString();
        }
    }
}
