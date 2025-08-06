namespace TestServer.Models.HomeViewModels
{
    public class WorkersCheckInViewModel
    {
        public WorkersCheckInViewModel()
        {
            Workers = new List<WorkerViewModel>();
            WorkerInfo = new List<WorkerStoneViewModel>();
            WorkerDataCurretDate = new List<WorkerStoneViewModel>();
        }
        public List<WorkerViewModel> Workers { get; set; }
        public StoneViewModel Stones { get; set; }
        public List<WorkerStoneViewModel> WorkerInfo { get; set; }
        public List<WorkerStoneViewModel> WorkerDataCurretDate { get; set; }
        public WorkerStoneViewModel EditableData { get; set; }
    }
}
