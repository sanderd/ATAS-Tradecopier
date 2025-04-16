using Microsoft.AspNetCore.Mvc;
using sadnerd.io.ATAS.OrderEventHub.Data.Services;

namespace sadnerd.io.ATAS.OrderEventHub.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CopyStrategyController : ControllerBase
{
    private readonly CopyStrategyService _copyStrategyService;

    public CopyStrategyController(CopyStrategyService copyStrategyService)
    {
        _copyStrategyService = copyStrategyService;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _copyStrategyService.DeleteStrategy(id);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Strategy not found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
