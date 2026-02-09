using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Catalog;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public sealed class ProductsController : ControllerBase
    {
        private readonly ICatalogService _catalog;

        public ProductsController(ICatalogService catalog)
        {
            _catalog = catalog;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ProductListItemResponse>>> GetActiveProducts(CancellationToken cancellationToken = default)
        {
            var res = await _catalog.GetActiveProductsAsync(cancellationToken);
            return Ok(res);
        }

        [Authorize]
        [HttpGet("{productId:guid}/content")]
        public async Task<ActionResult<ProductContentResponse>> GetContent([FromRoute] Guid productId, CancellationToken cancellationToken = default)
        {
            var res = await _catalog.GetProductContentAsync(productId, cancellationToken);
            return Ok(res);
        }
    }
}
