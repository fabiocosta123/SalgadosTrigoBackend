using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalgadosTrigo.Api.Authorization;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Services.Catalog;

namespace SalgadosTrigo.Api.Controllers.Catalog
{
    [ApiController]
    [Authorize(Policy = RolePolicies.Suppliers)]
    [Route("api/suppliers")]
    public class SuppliersController(SupplierService suppliers) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SupplierResponse>>> List()
        {
            return Ok(await suppliers.List());
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<SupplierResponse>> Get(long id)
        {
            var supplier = await suppliers.Find(id);

            if (supplier is null)
            {
                return Problem(
                    title: "Fornecedor não encontrado",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(supplier);
        }

        [HttpPost]
        public async Task<ActionResult<SupplierResponse>> Create(SupplierRequest request)
        {
            var createdById = CurrentUserId();
            if (createdById is null)
            {
                return Problem(
                    title: "Não foi possível cadastrar o fornecedor",
                    detail: "Sessão sem usuário.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await suppliers.Register(request, createdById.Value);

            if (result.Supplier is null)
            {
                var status = result.Error == SupplierService.DuplicateDocumentMessage
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status400BadRequest;

                return Problem(
                    title: "Não foi possível cadastrar o fornecedor",
                    detail: result.Error,
                    statusCode: status);
            }

            return CreatedAtAction(nameof(Get), new { id = result.Supplier.Id }, result.Supplier);
        }

        private long? CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id) ? id : null;
        }
    }
}
