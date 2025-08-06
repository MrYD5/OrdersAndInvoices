using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using TestServer.Data;
using TestServer.Models.HomeViewModels;
using TestServer.Services.HomeService;

namespace TestServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IWorkerService _workerService;
        private readonly IStoneService _stoneService;
        private readonly IDatabaseService _databaseService;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, IInvoiceService invoiceService, IInvoiceService invoiceSerices, IWorkerService workerService, IStoneService stoneService, IDatabaseService databaseService)
        {
            _invoiceService = invoiceService;
            _logger = logger;
            _env = env;
            _workerService = workerService;
            _stoneService = stoneService;
            _databaseService = databaseService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteImage(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var path = Path.Combine(@"C:\AppUploads\Invoices\" + fileName);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            TempData["GalleryOpen"] = true;
            return RedirectToAction("Upload");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Table()
        {
            if (TempData["Invoices"] is string json)
            {
                var invoices = System.Text.Json.JsonSerializer.Deserialize<List<InvoiceViewModel>>(json);
                return View(invoices);
            }

            return View(new List<InvoiceViewModel>());
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetInvoiceImage(string fileName)
        {
            string directory = @"C:\AppUploads\Invoices";
            string path = Path.Combine(directory, fileName);

            if (!System.IO.File.Exists(path))
                return NotFound();

            string contentType = "image/jpeg";
            if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                contentType = "image/png";

            byte[] imageBytes = System.IO.File.ReadAllBytes(path);
            return File(imageBytes, contentType);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Check")]
        public IActionResult SubmitProducts(WorkersCheckInViewModel data)
        {
            foreach (var item in data.WorkerInfo)
            {
                if (_workerService.DoesCheckInExist(item))
                {
                    _workerService.ChangeCheckIn(item);
                }
                else
                {
                    _workerService.AddDataForCurrentDate(item);
                }
            }
            return RedirectToAction("Workers", "Home");
        }
        [Authorize(Roles = "Admin,Check")]
        public IActionResult Workers()
        {
            WorkersCheckInViewModel model = new WorkersCheckInViewModel
            {
                Workers = _workerService.GetWorkerViewModels().ToList(),
                Stones = _stoneService.GetStoneViewModels(),
                WorkerDataCurretDate = _workerService.GetDataByCurrentDate(),
            };
            if (TempData["Edit"] != null)
            {
                WorkerStoneViewModel editableData = null;
                if (TempData["Edit"] is string serializedModel)
                {
                    editableData = JsonConvert.DeserializeObject<WorkerStoneViewModel>(serializedModel);
                }
                model.EditableData = editableData;
                TempData.Clear();
            }

            if (User.IsInRole("Admin"))
            {
                TempData["Role"] = "Admin";
            }

            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult LoadInvoice()
        {
            return Redirect("Table");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Finish()
        {
            _invoiceService.DeleteAllInvoices();
            return Redirect("\\");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult EditInvoice()
        {
            var data = _invoiceService.EditInvoicesAsync().Result;
            TempData["Invoices"] = System.Text.Json.JsonSerializer.Serialize(data);
            return RedirectToAction("Table");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult UploadInvoice(List<InvoiceViewModel> model)
        {
            _invoiceService.GenerateExcelFile(model);

            if (model.Count == 0)
            {
                return Redirect("\\");
            }
            var path = @"C:\AppUploads\Invoices\faktura.xlsx";
            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "Фактура.xlsx";

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);

            _invoiceService.DeleteAllInvoices();

            return File(fileBytes, mimeType, fileName);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage(List<IFormFile> imageFiles)
        {
            var result = await _invoiceService.AddInvoice(imageFiles);

            if (result.Any())
            {
                TempData["Message"] = "Файловете са качени успешно.";
            }
            else
            {
                TempData["Message"] = "Не са избрани валидни файлове.";
            }

            return RedirectToAction("Upload", "Home");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult LoadImages()
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var imagePaths = Directory
                .GetFiles(uploadsFolder)
                .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => "/uploads/" + Path.GetFileName(f))
                .ToList();

            return PartialView("_ImageList", imagePaths);
        }
        [Authorize(Roles = "Admin,Check")]
        public IActionResult UpdateRow(WorkerStoneViewModel model)
        {
            if (_workerService.DoesCheckInExist(model))
            {
                var temp = new WorkerStoneViewModel()
                {
                    Id = model.Id,
                    Worker = model.Worker,
                    Color = model.Color,
                    Amount = model.Amount,
                    SelectedType = model.SelectedType,
                };

                TempData["Edit"] = JsonConvert.SerializeObject(temp);

                return RedirectToAction("Workers", "Home");
            }

            _workerService.UpdateRow(model);
            return RedirectToAction("Workers", "Home");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Database()
        {
            var result = _databaseService.GetWorkersCheckIns();
            if (TempData["Filter"] != null)
            {
                if (TempData["DateToDate"] != null && TempData["DateToDate"].ToString() == "True")
                {
                    result.CheckIns = _databaseService.GetFilteredCheckIns(TempData["Filter"].ToString(), true).ToList();
                }
                else
                {
                    result.CheckIns = _databaseService.GetFilteredCheckIns(TempData["Filter"].ToString(), false).ToList();
                }  
            }
            TempData.Clear();
                return this.View(result);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult SearchDatabase(SearchViewModel model)
        {
            if (model.MainSelect != null && model.FilterValue != null)
            {
                TempData["Filter"] = model.MainSelect.ToString() + '/' + model.FilterValue.ToString();

                if (model.SecondarySelect != null && model.SecondarySelect != null)
                {
                    TempData["Filter"] += '/' + model.SecondarySelect.ToString() + '/' + model.SecondFilterValue.ToString();
                }
            }
            if (model.SearchDateToDate)
            {
                TempData["DateToDate"] = "True";
            }
            return RedirectToAction("Database", "Home");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AddData(AddDataViewModel data)
        {
            if (data.DataEntry != null && data.Value != null)
            {
                if (data.DataEntry == "Worker")
                {
                    if(_databaseService.AddWorker(new Worker
                    {
                        Name = data.Value,
                    }))
                    {
                        TempData["Message"] = "Вече има такъв запис!";
                    }
                }
                else if (data.DataEntry == "Color")
                {
                    if(_databaseService.AddStoneColor(new StoneColor
                    {
                        Color = data.Value,
                    }))
                    {
                        TempData["Message"] = "Вече има такъв запис!";
                    }
                }
                else
                {
                    if(_databaseService.AddStoneType(new StoneType
                    {
                        Type = data.Value,
                    }))
                    {
                        TempData["Message"] = "Вече има такъв запис!";
                    }
                }
            }

            var simpleData = _databaseService.GetSimpleData();

            return this.View(simpleData);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteWorker(int id)
        {
            _databaseService.FireWorker(id);
            return this.RedirectToAction("AddData", "Home");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteColor(int id)
        {
            _databaseService.DeleteStoneColor(id);
            return this.RedirectToAction("AddData", "Home");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteType(int id)
        {
            _databaseService.DeleteStoneType(id);
            return this.RedirectToAction("AddData", "Home");
        }

        public IActionResult CheckInsExcel()
        {
            var file = _workerService.GenerateCheckInExcelFile(_workerService.GetDataByCurrentDate());

            return File(file,
           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
           "Отчет.xlsx");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
