using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalgadosTrigo.Api.Authorization;
using SalgadosTrigo.Api.Contracts.Stock;
using SalgadosTrigo.Api.Services.Stock;

namespace SalgadosTrigo.Api.Controllers.Stock
{
    [ApiController]
    [Authorize(Policy = RolePolicies.Suppliers)]
    [Route("api/purchase-entries")]
    public class PurchaseEntriesController(PurchaseEntryService entries) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<PurchaseEntryResponse>> Create(PurchaseEntryRequest request)
        {
            var createdById = CurrentUserId();
            if (createdById is null)
            {
                return Problem(
                    title: "Não foi possível lançar a compra",
                    detail: "Sessão sem usuário.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await entries.Register(request, createdById.Value);
            if (result.Entry is null)
            {
                return Problem(
                    title: "Não foi possível lançar a compra",
                    detail: result.Error,
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return Created($"/api/purchase-entries/{result.Entry.Id}", result.Entry);
        }

        private long? CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id) ? id : null;
        }
    }
}