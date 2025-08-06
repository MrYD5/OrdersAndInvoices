using TestServer.Data;
using TestServer.Models.HomeViewModels;

namespace TestServer.Services.HomeService
{
    public interface IDatabaseService
    {
        public DetailedWorkerStoneViewModel GetWorkersCheckIns();
        public List<WorkerStoneViewModel> GetFilteredCheckIns(string filter, bool dateToDate);
        public bool AddWorker(Worker worker);
        public bool AddStoneColor (StoneColor color);
        public bool AddStoneType(StoneType type);
        public SimpleDataViewModel GetSimpleData();
        public void FireWorker(int id);
        public void DeleteStoneColor(int id);
        public void DeleteStoneType(int id);

    }
}
