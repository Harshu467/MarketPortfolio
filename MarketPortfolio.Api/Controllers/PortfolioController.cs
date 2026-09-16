using MarketPortfolio.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketPortfolio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioManagerService _managerService;

        // Constructor Injection: Both services are automatically provided by the DI container
        public PortfolioController(PortfolioManagerService managerService)
        {
            _managerService = managerService;
        }

        [HttpPost("stocks")]
        public async Task<IActionResult> AddStock([FromBody] AddStockRequest request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); 

            try
            {
                await _managerService.AddStockToPortfolioAsync(
                    userId, 
                    request.Ticker, 
                    request.PurchasePrice, 
                    request.Shares, 
                    cancellationToken
                );

                return Ok(new { Message = $"Successfully added {request.Shares} shares of {request.Ticker}." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // --- NEW GET ENDPOINT FOR LIVE VALUATION ---
        [HttpGet("value")]
        public async Task<IActionResult> GetPortfolioValue(CancellationToken cancellationToken)
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            try
            {
                // Calls the external market client via the service layer
                decimal totalValue = await _managerService.CalculateTotalPortfolioValueAsync(userId, cancellationToken);

                return Ok(new { 
                    UserId = userId, 
                    Currency = "USD", 
                    TotalNetWorth = totalValue,
                    Timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to calculate live portfolio value.", Details = ex.Message });
            }
        }
    }

    public class AddStockRequest
    {
        public string Ticker { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public int Shares { get; set; }
    }
}