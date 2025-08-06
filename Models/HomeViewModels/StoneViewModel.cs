namespace TestServer.Models.HomeViewModels
{
    public class StoneViewModel
    {
        public StoneViewModel()
        {
            StoneColor = new List<string>();
            StoneType = new List<string>();
        }
        public List<string> StoneColor { get; set; }
        public List<string> StoneType { get; set; }
    }
}
