using MarketPortfolio.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketPortfolio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioManagementService _portfolioService;

        // Constructor Injection: The DI container provides the service automatically
        public PortfolioController(PortfolioManagementService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        [HttpPost("stocks")]
        public async Task<IActionResult> AddStock([FromBody] AddStockRequest request, CancellationToken cancellationToken)
        {
            // In the real app, we will extract this from the JWT token claims.
            // For testing the architecture now, we simulate a logged-in user ID.
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); 

            try
            {
                await _portfolioService.AddStockToPortfolioAsync(
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
                // This catches our business rule violation (e.g., "Free tier limit reached")
                // and translates it into an HTTP 400 Bad Request.
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // Catch-all for unexpected database or server errors
                return StatusCode(500, new { 
                    Error = ex.Message, 
                    InnerDetails = ex.InnerException?.Message 
                });
            }
        }
    }

    // Data Transfer Object (DTO): Defines the exact JSON structure the client must send.
    // Placing this in the API layer keeps the Domain layer free of HTTP concerns.
    public class AddStockRequest
    {
        public string Ticker { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public int Shares { get; set; }
    }
}