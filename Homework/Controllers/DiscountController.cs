using Homework.DTOs.Discount;
using Homework.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/discounts")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountsController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDiscount([FromBody] AddDiscount discount)
    {
        var createdDiscount = await _discountService.CreateDiscountAsync(discount);
        return CreatedAtAction(nameof(GetDiscount), new { id = createdDiscount.Id }, createdDiscount);
    }

    [HttpGet]
    public async Task<IActionResult> GetDiscounts()
    {
        var discounts = await _discountService.GetDiscountsAsync();
        return Ok(discounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDiscount(int id)
    {
        var discount = await _discountService.GetDiscountByIdAsync(id);
        return Ok(discount);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDiscount(int id, [FromBody] Discount updatedDiscount)
    {
        updatedDiscount.Id = id;
        var success = await _discountService.UpdateDiscountAsync(updatedDiscount);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDiscount(int id)
    {
        var success = await _discountService.DeleteDiscountAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}
