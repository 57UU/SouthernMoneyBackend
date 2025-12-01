using Microsoft.AspNetCore.Mvc;
using SouthernMoneyBackend.Utils;
using SouthernMoneyBackend.Middleware;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/transaction")]
[AuthorizeUser]
public class TransactionController : ControllerBase
{
    public TransactionController()
    {
    }

    // POST /transaction/buy
    [HttpPost("buy")]
    public async Task<ApiResponse<object>> BuyProduct([FromBody] BuyProductRequest request)
    {
        throw new NotImplementedException();
    }

    // GET /transaction/myRecords?page={page}&pageSize={pageSize}
    [HttpGet("myRecords")]
    public async Task<ApiResponse<PaginatedResponse<PurchaseRecordsPageDto>>> GetMyRecords(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        throw new NotImplementedException();
    }
}
