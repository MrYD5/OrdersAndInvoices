using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Data;
using System.Globalization;
using TestServer.Data;
using TestServer.Models.HomeViewModels;

namespace TestServer.Services.HomeService
{
    public class DatabaseService : IDatabaseService
    {
        private readonly GoceGidContext db;
        public DatabaseService(GoceGidContext context)
        {
            db = context;
        }
        public bool AddStoneColor(StoneColor color)
        {
            if (!db.StoneColors.Any(x => x.Color == color.Color))
            {
                db.StoneColors.Add(color);
                db.SaveChanges();
                return false;
            }
            return true;
        }

        public bool AddStoneType(StoneType type)
        {
            if (!db.StoneTypes.Any(x => x.Type == type.Type))
            {
                db.StoneTypes.Add(type);
                db.SaveChanges();
                return false;
            }
            return true;
        }

        public bool AddWorker(Worker worker)
        {
            if (!db.Workers.Any(x => x.Name == worker.Name))
            {
                db.Workers.Add(worker);
                db.SaveChanges();
                return false;
            }
           //else
           //{
           //    var data = db.Workers.Where(x => x.Name == worker.Name).FirstOrDefault();
           //    data.HasQuit = false;
           //    db.SaveChanges();
           //}
                return true;
        }

        public void DeleteStoneColor(int id)
        {
            var data = db.StoneColors.Where(x => x.Id == id).FirstOrDefault();
            db.StoneColors.Remove(data);
            db.SaveChanges();
        }

        public void DeleteStoneType(int id)
        {
            var data = db.StoneTypes.Where(x => x.Id == id).FirstOrDefault();
            db.StoneTypes.Remove(data);
            db.SaveChanges();
        }

        public void FireWorker(int id)
        {
            var data = db.Workers.Where(x => x.Id == id).FirstOrDefault();
            if (data != null)
            {
                data.HasQuit = true;
                db.SaveChanges();
            }
        }

        public List<WorkerStoneViewModel> GetFilteredCheckIns(string filter, bool dateToDate)
        {
            var search = filter.Split('/');
            bool secondFilterRepeat = false;
            bool secondFilter = false;
            if (search.Length > 2)
            {
                if (search[0] == search[2])
                {
                    secondFilterRepeat = true;
                }

                secondFilter = true;
            }

            IQueryable<CheckIn> query = db.CheckIns;
            if (dateToDate)
            {
                var date1 = DateTime.ParseExact(search[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var date2 = DateTime.ParseExact(search[3], "yyyy-MM-dd", CultureInfo.InvariantCulture);

                DateTime startDate;
                DateTime endDate;

                if (DateTime.Compare(date1, date2) < 0)
                {
                    startDate = date1;
                    endDate = date2;
                }
                else
                {
                    startDate = date2;
                    endDate = date1;
                }

                query = query.Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date);
            }

            else if (!secondFilterRepeat)
            {
                switch (search[0])
                {
                    case "WorkerName":
                        query = query.Where(x => x.WorkerName == search[1]);
                        break;
                    case "Amount":
                        query = query.Where(x => x.Amount == int.Parse(search[1]));
                        break;
                    case "Color":
                        query = query.Where(x => x.Color == search[1]);
                        break;
                    case "Type":
                        query = query.Where(x => x.Type == search[1]);
                        break;
                    case "Date":
                        var date = DateTime.ParseExact(search[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        query = query.Where(x => x.Date.Date == date.Date);
                        break;
                }
                if (secondFilter)
                {
                    switch (search[2])
                    {
                        case "WorkerName":
                            query = query.Where(x => x.WorkerName == search[3]);
                            break;
                        case "Amount":
                            query = query.Where(x => x.Amount == int.Parse(search[3]));
                            break;
                        case "Color":
                            query = query.Where(x => x.Color == search[3]);
                            break;
                        case "Type":
                            query = query.Where(x => x.Type == search[3]);
                            break;
                        case "Date":
                            var date = DateTime.ParseExact(search[3], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            query = query.Where(x => x.Date.Date == date.Date);
                            break;
                    }
                }
            }
            else
            {
                switch (search[0])
                {
                    case "WorkerName":
                        query = query.Where(x => x.WorkerName == search[1] || x.WorkerName == search[3]);
                        break;
                    case "Amount":
                        query = query.Where(x => x.Amount == int.Parse(search[1]) || x.Amount == int.Parse(search[3]));
                        break;
                    case "Color":
                        query = query.Where(x => x.Color == search[1] || x.Color == search[3]);
                        break;
                    case "Type":
                        query = query.Where(x => x.Type == search[1] || x.Type == search[3]);
                        break;
                    case "Date":
                        var date = DateTime.ParseExact(search[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        var secondDate = DateTime.ParseExact(search[3], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        query = query.Where(x => x.Date.Date == date.Date || x.Date.Date == secondDate.Date);
                        break;
                }
            }

            
            var result = query.Select(x => new WorkerStoneViewModel
            {
                Worker = x.WorkerName,
                Color = x.Color,
                Amount = x.Amount,
                SelectedType = x.Type,
                Date = x.Date
            }).ToList();

            return result;
        }

        public SimpleDataViewModel GetSimpleData()
        {
            return new SimpleDataViewModel() 
            { 
                Workers = db.Workers.Select(x => new Worker()
                {
                    Id = x.Id,
                    Name = x.Name,
                }).ToList(),
                StoneColors = db.StoneColors.Select(x => new StoneColor()
                {
                    Id = x.Id,
                    Color = x.Color,
                }).ToList(),
                StoneTypes = db.StoneTypes.Select(x => new StoneType()
                {
                    Id = x.Id,
                    Type = x.Type,
                }).ToList(),

            };
        }

        public DetailedWorkerStoneViewModel GetWorkersCheckIns()
        {
            var data = new DetailedWorkerStoneViewModel();
            data.CheckIns = db.CheckIns.Select(x => new WorkerStoneViewModel
            {
                Id = x.Id,
                Worker = x.WorkerName,
                Amount = x.Amount,
                Color = x.Color,
                SelectedType = x.Type,
                Date = x.Date,
            }).ToList();

            data.Workers = db.Workers.Select(x => x.Name).ToList();
            data.Stones.StoneColor = db.StoneColors.Select(x => x.Color).ToList();
            data.Stones.StoneType = db.StoneTypes.Select(x => x.Type).ToList();

            return data;
        }
    }
}
