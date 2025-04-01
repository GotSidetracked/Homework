using Homework.Models;

public interface IInvoiceService
{
    Task<Invoice> CreateInvoiceAsync(Invoice invoice);
    Task<List<Invoice>> GetInvoicesAsync();
    Task<Invoice> GetInvoiceByIdAsync(int id);
    Task<bool> UpdateInvoiceAsync(Invoice invoice);
    Task<bool> DeleteInvoiceAsync(int id);
}
