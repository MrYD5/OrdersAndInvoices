using TestServer.Data;
using TestServer.Models.HomeViewModels;

namespace TestServer.Services.HomeService
{
    public interface IWorkerService
    {
        public List<WorkerViewModel> GetWorkerViewModels();
        public List<WorkerStoneViewModel> GetDataByCurrentDate();
        public void AddDataForCurrentDate(WorkerStoneViewModel data);
        public CheckIn GetDataById(int id);
        public void UpdateRow(WorkerStoneViewModel data);
        public bool DoesCheckInExist(WorkerStoneViewModel data);
        public void ChangeCheckIn(WorkerStoneViewModel data);
        public byte[] GenerateCheckInExcelFile(List<WorkerStoneViewModel> data);
    }
}
