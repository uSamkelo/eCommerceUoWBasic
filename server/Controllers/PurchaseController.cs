using eCommerceUoWBasic.Models;
using eCommerceUoWBasic.Services;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceUoWBasic.Controllers;

[ApiController]
[Route("api/purchases")]
public sealed class PurchaseController(PurchaseService purchaseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Purchase(
        [FromBody] IReadOnlyCollection<PurchaseRequest>? requests,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (requests is null || requests.Count == 0)
        {
            return BadRequest(new { message = "At least one purchase is required." });
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(new { message = "The Idempotency-Key header is required." });
        }

        try
        {
            var outcome = await purchaseService.TryPurchaseAsync(
                requests,
                idempotencyKey.Trim(),
                cancellationToken);

            return StatusCode(outcome.StatusCode, outcome.Response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}