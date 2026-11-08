using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalgadosTrigo.Api.Contracts.Catalog;
using SalgadosTrigo.Api.Services.Catalog;

namespace SalgadosTrigo.Api.Controllers.Catalog
{
    [ApiController]
    [Authorize]
    [Route("api/recipes")]
    public class RecipesController(RecipeService recipes) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RecipeResponse>>> List()
        {
            return Ok(await recipes.List());
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<RecipeResponse>> Get(long id)
        {
            var recipe = await recipes.Find(id);

            if (recipe is null)
            {
                return Problem(
                    title: "Ficha não encontrada",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(recipe);
        }

        [HttpPost]
        public async Task<ActionResult<RecipeResponse>> Create(RecipeRequest request)
        {
            var result = await recipes.Register(request);

            if (result.Recipe is null)
            {
                return ToActionResult(result);
            }

            return CreatedAtAction(nameof(Get), new { id = result.Recipe.Id }, result.Recipe);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<RecipeResponse>> Update(long id, RecipeRequest request)
        {
            var result = await recipes.Update(id, request);
            return ToActionResult(result);
        }

        private ActionResult<RecipeResponse> ToActionResult(RecipeRegistration result)
        {
            if (result.Recipe is not null)
            {
                return Ok(result.Recipe);
            }

            var status = result.Error switch
            {
                RecipeService.NotFoundMessage => StatusCodes.Status404NotFound,
                RecipeService.AlreadyExistsMessage => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return Problem(
                title: "Não foi possível gravar a ficha",
                detail: result.Error,
                statusCode: status);
        }
    }
}
