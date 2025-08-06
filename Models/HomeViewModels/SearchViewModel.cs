using System.Reflection.Metadata.Ecma335;

namespace TestServer.Models.HomeViewModels
{
    public class SearchViewModel
    {
        public string MainSelect { get; set; }
        public string FilterValue { get; set; }
        public string SecondarySelect { get; set; }
        public string SecondFilterValue { get; set; }
        public bool SearchDateToDate { get; set; }
    }
}
