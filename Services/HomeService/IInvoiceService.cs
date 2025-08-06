using TestServer.Data;
using TestServer.Models.HomeViewModels;

namespace TestServer.Services.HomeService
{
    public interface IInvoiceService
    {
        Task<List<string>> AddInvoice(List<IFormFile> files);
        public Task<List<InvoiceViewModel>> EditInvoicesAsync();
        public void GenerateExcelFile(List<InvoiceViewModel> data);
        public void DeleteAllInvoices();
    }
}
