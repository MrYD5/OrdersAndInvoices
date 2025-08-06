using DocumentFormat.OpenXml.Office2010.PowerPoint;
using TestServer.Data;
using TestServer.Models.HomeViewModels;

namespace TestServer.Services.HomeService
{
    public class StoneService : IStoneService
    {
        private readonly GoceGidContext db;
        public StoneService(GoceGidContext contex)
        {
                db = contex;
        }
        public StoneViewModel GetStoneViewModels()
        {
            return new StoneViewModel()
            {
                StoneColor = db.StoneColors.Select(x => x.Color).OrderBy(x => x).ToList(),
                StoneType = db.StoneTypes.Select(x => x.Type).OrderByDescending(x => x).ToList(),
            };
        }
    }
}
