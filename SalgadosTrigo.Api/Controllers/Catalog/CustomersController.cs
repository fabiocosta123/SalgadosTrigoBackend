using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Services.Catalog;

namespace SalgadosTrigo.Api.Controllers.Catalog
{
    [ApiController]
    [Authorize]
    [Route("api/customers")]
    public class CustomersController(CustomerService customers) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> List()
        {
            return Ok(await customers.List());
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<CustomerResponse>> Get(long id)
        {
            var customer = await customers.Find(id);

            if (customer is null)
            {
                return Problem(
                    title: "Cliente não encontrado",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerResponse>> Create(CustomerRequest request)
        {
            var createdById = CurrentUserId();
            if (createdById is null)
            {
                return Problem(
                    title: "Não foi possível cadastrar o cliente",
                    detail: "Sessão sem usuário.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await customers.Register(request, createdById.Value);

            if (result.Customer is null)
            {
                var status = result.Error == CustomerService.DuplicateDocumentMessage
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status400BadRequest;

                return Problem(
                    title: "Não foi possível cadastrar o cliente",
                    detail: result.Error,
                    statusCode: status);
            }

            return CreatedAtAction(nameof(Get), new { id = result.Customer.Id }, result.Customer);
        }

        private long? CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id) ? id : null;
        }
    }
}
