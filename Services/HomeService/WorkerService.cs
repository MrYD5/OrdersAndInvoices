using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using TestServer.Data;
using TestServer.Models.HomeViewModels;

namespace TestServer.Services.HomeService
{
    public class WorkerService : IWorkerService
    {
        private readonly GoceGidContext db;
        public WorkerService(GoceGidContext context)
        {
                db = context;
        }
        public void AddDataForCurrentDate(WorkerStoneViewModel data)
        {
            if (data.Worker != null && data.Color != null && data.Amount != 0 && data.SelectedType != null)
            {
                db.CheckIns.Add(new CheckIn
                {
                    WorkerName = data.Worker,
                    Color = data.Color,
                    Amount = data.Amount,
                    Type = data.SelectedType,
                    Date = DateTime.Now,
                });
                db.SaveChanges();
            }
        }

        public void ChangeCheckIn(WorkerStoneViewModel data)
        {
            var checkIn = db.CheckIns.Where(x => x.Id == data.Id).FirstOrDefault();

            if (checkIn != null)
            {
                checkIn.WorkerName = data.Worker;
                checkIn.Color = data.Color;
                checkIn.Amount = data.Amount;
                checkIn.Type = data.SelectedType;
                checkIn.Date = DateTime.Now;
                db.SaveChanges();
            } 
        }

        public bool DoesCheckInExist(WorkerStoneViewModel data)
        {
            return db.CheckIns.Any(x => x.Id == data.Id);
        }

        public byte[] GenerateCheckInExcelFile(List<WorkerStoneViewModel> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Камъни разпределение");

            // Заглавия
            worksheet.Cell(1, 1).Value = "Камък (Цвят)";
            worksheet.Cell(1, 2).Value = "Камък (Вид)";
            worksheet.Cell(1, 3).Value = "Количество";
            worksheet.Cell(1, 4).Value = "Работник";

            for (int i = 0; i < data.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = data[i].Worker;
                worksheet.Cell(i + 2, 2).Value = data[i].Color;
                worksheet.Cell(i + 2, 3).Value = data[i].Amount;
                worksheet.Cell(i + 2, 4).Value = data[i].SelectedType;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public List<WorkerStoneViewModel> GetDataByCurrentDate()
        {
            return db.CheckIns.Where(x => x.Date.Day == DateTime.Now.Day && x.Date.Month == DateTime.Now.Month && x.Date.Year == DateTime.Now.Year).Select(x => new WorkerStoneViewModel
            {
                Id = x.Id,
                Worker = x.WorkerName,
                Color = x.Color,
                Amount = x.Amount,
                SelectedType = x.Type,
            }).ToList();
        }

        public CheckIn GetDataById(int id)
        {
            return db.CheckIns.Where(x => x.Id == id).FirstOrDefault();
        }

        public List<WorkerViewModel> GetWorkerViewModels()
        {
            return db.Workers.Where(x => x.HasQuit == false).Select(x => new WorkerViewModel
            {
                Name = x.Name,
            }).OrderBy(x => x.Name).ToList();
        }

        public void UpdateRow(WorkerStoneViewModel data)
        {
            var row = db.CheckIns.Where(x => x.Id == data.Id).FirstOrDefault();
            if (row != null)
            {
                row.WorkerName = data.Worker;
                row.Color = data.Color;
                row.Amount = data.Amount;
                row.Type = data.SelectedType;
                db.SaveChanges();
            }
        }
    }
    
}
