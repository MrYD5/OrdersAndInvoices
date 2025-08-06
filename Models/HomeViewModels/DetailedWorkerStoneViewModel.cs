using System.Reflection.Metadata.Ecma335;
using TestServer.Data;

namespace TestServer.Models.HomeViewModels
{
    public class DetailedWorkerStoneViewModel
    {
        public DetailedWorkerStoneViewModel()
        {
            CheckIns = new List<WorkerStoneViewModel>();
            Workers = new List<string>();
            Stones = new StoneViewModel();
        }
        public List<WorkerStoneViewModel> CheckIns { get; set; }
        public List<string> Workers { get; set; }
        public StoneViewModel Stones { get; set; }
    }
}
