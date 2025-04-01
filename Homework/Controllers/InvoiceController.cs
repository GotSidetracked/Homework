using Homework.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] Invoice invoice)
    {
        var createdInvoice = await _invoiceService.CreateInvoiceAsync(invoice);
        return CreatedAtAction(nameof(GetInvoice), new { id = createdInvoice.Id }, createdInvoice);
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices()
    {
        var invoices = await _invoiceService.GetInvoicesAsync();
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        return Ok(invoice);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoice(int id, [FromBody] Invoice updatedInvoice)
    {
        updatedInvoice.Id = id;
        var success = await _invoiceService.UpdateInvoiceAsync(updatedInvoice);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var success = await _invoiceService.DeleteInvoiceAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}
