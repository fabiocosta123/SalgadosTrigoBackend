using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalgadosTrigo.Api.Authorization;
using SalgadosTrigo.Api.Contracts.Stock;
using SalgadosTrigo.Api.Services.Stock;

namespace SalgadosTrigo.Api.Controllers.Stock
{
    [ApiController]
    [Authorize(Policy = RolePolicies.Costs)]
    [Route("api/stock-balances")]
    public class StockBalancesController(StockBalanceService balances) : ControllerBase
    {
        [HttpGet("{productId:long}")]
        public async Task<ActionResult<StockBalanceResponse>> Get(long productId)
        {
            var result = await balances.Find(productId);
            if (result.Balance is null)
            {
                var status = result.Error == StockBalanceService.NotFoundMessage
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest;

                return Problem(
                    title: "Não foi possível consultar o saldo",
                    detail: result.Error,
                    statusCode: status);
            }

            return Ok(result.Balance);
        }
    }
}