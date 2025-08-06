using TestServer.Data;

namespace TestServer.Models.HomeViewModels
{
    public class SimpleDataViewModel
    {
        public List<Worker> Workers { get; set; }
        public List<StoneColor> StoneColors { get; set; }
        public List<StoneType> StoneTypes { get; set; }
    }
}
