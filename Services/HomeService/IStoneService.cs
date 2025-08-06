using TestServer.Models.HomeViewModels;

namespace TestServer.Services.HomeService
{
    public interface IStoneService
    {
        public StoneViewModel GetStoneViewModels();
    }
}
