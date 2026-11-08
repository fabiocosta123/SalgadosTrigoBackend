using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Services.Catalog;

namespace SalgadosTrigo.Api.Controllers.Catalog
{
    [ApiController]
    [Authorize]
    [Route("api/products")]
    public class ProductsController(ProductService products) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductResponse>>> List()
        {
            return Ok(await products.List());
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ProductResponse>> Get(long id)
        {
            var product = await products.Find(id);

            if (product is null)
            {
                return Problem(
                    title: "Produto não encontrado",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create(ProductRequest request)
        {
            var result = await products.Register(request);

            if (result.Product is null)
            {
                var status = result.Error == ProductService.DuplicateCodeMessage
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status400BadRequest;

                return Problem(
                    title: "Não foi possível cadastrar o produto",
                    detail: result.Error,
                    statusCode: status);
            }

            return CreatedAtAction(nameof(Get), new { id = result.Product.Id }, result.Product);
        }
    }
}
