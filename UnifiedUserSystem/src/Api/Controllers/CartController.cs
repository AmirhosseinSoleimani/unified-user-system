using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Order;

namespace UnifiedUserSystem.src.Api.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public sealed class CartController : ControllerBase
    {
        private readonly IOrderService _orders;

        public CartController(IOrderService orders)
        {
            _orders = orders;
        }

        [HttpGet]
        public async Task<ActionResult<CartResponse>> GetMyCart(CancellationToken cancellationToken = default)
        {
            var res = await _orders.GetMyOpenCartAsync(cancellationToken);
            return Ok(res);
        }

        [HttpPost("items")]
        public async Task<ActionResult<CartResponse>> AddToCart([FromBody] AddToCartRequest req, CancellationToken cancellationToken = default)
        {
            var res = await _orders.AddToMyCartAsync(req.ProductId, cancellationToken);
            return Ok(res);
        }

        [HttpPost("confirm")]
        public async Task<ActionResult<CartResponse>> Confirm(CancellationToken cancellationToken = default)
        {
            var res = await _orders.ConfirmMyCartAsync(cancellationToken);
            return Ok(res);
        }
    }
}
