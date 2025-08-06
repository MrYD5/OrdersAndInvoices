using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using TestServer.Data;
using TestServer.Models.HomeViewModels;

namespace TestServer.Services.HomeService
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IWebHostEnvironment _env;
        private readonly char[] dateSeparetors;
        private readonly IConfiguration _configuration;
        private readonly string apiKey;
        public InvoiceService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _configuration = configuration;
            _env = env;
            dateSeparetors = new char[]
            {
                '.','-',',','/','\\'
            };
            this.apiKey = _configuration["ApiKey"];
        }

        public async Task<List<string>> AddInvoice(List<IFormFile> files)
        {
            var uploadedFiles = new List<string>();

            if (files == null || files.Count == 0)
                return uploadedFiles;

            // Използваме външна папка, например C:\AppUploads
            var uploadsFolder = @"C:\AppUploads\Invoices";

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    var filePath = System.IO.Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // В този случай, връщаме само името на файла или пълния локален път ако е нужно
                    uploadedFiles.Add(fileName);
                }
            }

            return uploadedFiles;
        }
        public async Task<List<InvoiceViewModel>> EditInvoicesAsync()
        {
            string uploadsPath = @"C:\AppUploads\Invoices";

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var rawImages = Directory.GetFiles(uploadsPath);

            int counter = 0;
            foreach (var image in rawImages)
            {
                counter++;
                File.Move(image, System.IO.Path.Combine(uploadsPath, $"Фактура {counter}.jpg"));
            }

            string clientId = "9367b6e6410eda4";
            using var imageClient = new HttpClient();
            var imageList = new List<string>();

            for (int i = 1; i <= Directory.GetFiles(uploadsPath).Length; i++)
            {
                string imagePath = System.IO.Path.Combine(uploadsPath, $"Фактура {i}.jpg");
                imageClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", clientId);

                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                var imageContent = new MultipartFormDataContent
        {
            { new StringContent(base64Image), "image" }
        };

                var imageResponse = await imageClient.PostAsync("https://api.imgur.com/3/image", imageContent);
                string jsonResponse = await imageResponse.Content.ReadAsStringAsync();

                if (imageResponse.IsSuccessStatusCode)
                {
                    var link = JObject.Parse(jsonResponse)["data"]["link"].ToString();
                    imageList.Add(link);
                }
                else
                {
                    Console.WriteLine("Грешка при качване: " + jsonResponse);
                }
            }

            string question = "Извлечи от фактурата следните данни и ги подредиш така: име на продукта//количество (само число)//единична цена(само число)//сума без ДДС(само число)//име на купувач//име на продавач//номер на фактурата//дата. Ако има повече от един продукт, върни по един такъв ред за всеки и ги съедини с ';;' иначе в никакъв друг случай не слагай ';;'. В края на низът върни сумата за плащане(само число), пак разделена с '//'.";

            var requestBodyList = new List<object>();
            foreach (var image in imageList)
            {
                requestBodyList.Add(new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = question },
                        new { type = "image_url", image_url = new { url = image } }
                    }
                }
            },
                    max_tokens = 1000
                });
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var finalResult = new List<InvoiceViewModel>();

            foreach (var request in requestBodyList)
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                try
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    string reply = result?.choices?[0]?.message?.content;

                    if (reply.Contains(";;"))
                    {
                        Console.WriteLine(reply);
                        var parts = reply.Split(";;");
                        var priceData = parts.Last().Split("//");
                        decimal totalPrice = decimal.Parse(priceData.Last().Replace(',', '.'), CultureInfo.InvariantCulture);

                        foreach (var item in parts)
                        {
                            finalResult.Add(AddMultipleInvoice(item, totalPrice));
                        }
                    }
                    else
                    {
                        Console.WriteLine(reply);
                        finalResult.Add(AddOneInvoice(reply));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Неуспешно разчитане на отговор: " + ex.Message);
                }
            }

            foreach (var file in Directory.GetFiles(uploadsPath))
            {
                File.Delete(file);
            }

            return finalResult;
        }

        private InvoiceViewModel AddOneInvoice(string data)
        {
            var temp = data.Split("//").ToList();
            if (string.IsNullOrEmpty(temp[temp.Count - 1].ToString()) || string.IsNullOrEmpty(temp[temp.Count - 1].ToString()))
            {
                temp.Remove(temp[temp.Count - 1]);
            }
            for (int i = 1; i < 4; i++)
            {
                temp[i] = Regex.Replace(temp[i], @"\s+", "");
            }

            decimal price = decimal.Parse(temp[temp.Count - 1].Replace(',', '.'));
            DateOnly date = new DateOnly();
            if (!dateSeparetors.Any(c => dateSeparetors.Any(x => temp[7].Contains(x))))
            {
                date = DateOnly.ParseExact(temp[7], "ddMMyyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                date = DateOnly.ParseExact(temp[7].Replace('-', '.').Replace(',', '.').ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            InvoiceViewModel invoice = new InvoiceViewModel
                {
                    ProductName = temp[0].ToLower().ToString(),
                    Amount = double.Parse(temp[1].Replace(',', '.').ToString()),
                    SinglePrice = decimal.Parse(temp[2].Replace(',', '.').ToString()),
                    SumNoTax = decimal.Parse(temp[3].Replace(',', '.').ToString()),
                    SumToPay = price,
                    BuyerName = temp[4].ToString(),
                    SellerName = temp[5].ToString(),
                    InvoiceNumber = temp[6].ToString(),
                    Date = date,
                };
            return invoice;
        }

        private InvoiceViewModel AddMultipleInvoice(string data, decimal price)
        {
            var temp = data.Split("//").ToList();
            if (string.IsNullOrEmpty(temp[temp.Count - 1].ToString()) || string.IsNullOrEmpty(temp[temp.Count - 1].ToString()))
            {
                temp.Remove(temp[temp.Count - 1]);
            }
            for (int i = 1; i < 4; i++)
            {
                temp[i] = Regex.Replace(temp[i], @"\s+", "");
            }
            DateOnly date = new DateOnly();
            if (!dateSeparetors.Any(c => dateSeparetors.Any(x => temp[7].Contains(x))))
            {
                date = DateOnly.ParseExact(temp[7], "ddMMyyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                date = DateOnly.ParseExact(temp[7].Replace('-', '.').Replace(',', '.').ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            InvoiceViewModel invoice = new InvoiceViewModel
            {
                ProductName = temp[0].ToLower().ToString(),
                Amount = double.Parse(temp[1].Replace(',', '.').ToString()),
                SinglePrice = decimal.Parse(temp[2].Replace(',', '.').ToString()),
                SumNoTax = decimal.Parse(temp[3].Replace(',', '.').ToString()),
                SumToPay = price,
                BuyerName = temp[4].ToString(),
                SellerName = temp[5].ToString(),
                InvoiceNumber = temp[6].ToString(),
                Date = date,
            };
            return invoice;
        }

        public void GenerateExcelFile(List<InvoiceViewModel> model)
        {
            var invoice = Directory.GetFiles(@"C:\AppUploads\Invoices");
            if (invoice.Length != 0)
            {
                File.Delete(invoice[0]);
            }
            if (model.Count != 0)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Data");

                    // Заглавия (може да се пропуснат при нужда)
                    worksheet.Cell(1, 1).Value = "Име на Продукт";
                    worksheet.Cell(1, 2).Value = "Количество";
                    worksheet.Cell(1, 3).Value = "Единична Цена";
                    worksheet.Cell(1, 4).Value = "Цена без ДДС";
                    worksheet.Cell(1, 5).Value = "Цена за плащане";
                    worksheet.Cell(1, 6).Value = "Продавач";
                    worksheet.Cell(1, 7).Value = "Копувач";
                    worksheet.Cell(1, 8).Value = "Номер на фактурата";
                    worksheet.Cell(1, 9).Value = "Дата";

                    for (int i = 0; i < model.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = model[i].ProductName;
                        worksheet.Cell(i + 2, 2).Value = model[i].Amount;
                        worksheet.Cell(i + 2, 3).Value = model[i].SinglePrice;
                        worksheet.Cell(i + 2, 4).Value = model[i].SumNoTax;
                        worksheet.Cell(i + 2, 5).Value = model[i].SumToPay;
                        worksheet.Cell(i + 2, 6).Value = model[i].BuyerName;
                        worksheet.Cell(i + 2, 7).Value = model[i].SellerName;
                        worksheet.Cell(i + 2, 8).Value = model[i].InvoiceNumber;
                        worksheet.Cell(i + 2, 9).Value = model[i].Date.ToString();
                    }

                    workbook.SaveAs(@"C:\AppUploads\Invoices\faktura.xlsx");

                    //Console.WriteLine(string.Join(", ",item.ProductName, item.Amount, item.SinglePrice.ToString(), item.SumNoTax, item.SumToPay, item.BuyerName, item.SellerName, item.InvoiceNumber, item.Date));
                }
            }
        }

        public void DeleteAllInvoices()
        {
            var files = Directory.GetFiles(@"C:\AppUploads\Invoices");

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}