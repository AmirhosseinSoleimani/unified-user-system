using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Order;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Ordering.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.src.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IOrderBusiness _orderBusiness;
        private readonly ICurrentUser _currentUser;
        private readonly IClock _clock;

        public OrderService(
            IUnitOfWork uow,
            IOrderBusiness orderBusiness,
            ICurrentUser currentUser,
            IClock clock)
        {
            _uow = uow;
            _orderBusiness = orderBusiness;
            _currentUser = currentUser;
            _clock = clock;
        }

        public async Task<CartResponse> AddToMyCartAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            _orderBusiness.ValidateAddToCart(new AddToCartRequest { ProductId = productId, });

            var userId = _currentUser.UserId;
            Guard.True(userId is not null, "User is not authenticate");

            var product = await _uow.Products.FindByIdAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException("Product not found.");

            if(!product.IsActive)
                throw new InvalidOperationException("Product is not active.");

            var order = await _uow.Orders.FindOpenOrderForUSerAsync(userId.Value, cancellationToken);
            if (order is null) 
            {
                order = Order.CreateForUser(userId.Value, _clock.Utcnow, actorUserId: userId.Value);
                _uow.Orders.Add(order);
            }

            order.AddItem(product.Id, product.Price, _clock.Utcnow, actorUserId: userId.Value);

            await _uow.SaveChangesAsync();
            return ToCartResponse(order);
        }

        public async Task<CartResponse> ConfirmMyCartAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            Guard.True(userId is not null, "User is not authenticated.");

            var order = await _uow.Orders.FindOpenOrderForUSerAsync(userId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Open cart not found.");

            order.Confirm(_clock.Utcnow, actorUserId: userId.Value);

            var productIds = order.GetProductIds();
            foreach (var pid in productIds) 
            {
                await _uow.ProductUsers.AddAsync(userId.Value, pid, cancellationToken);
            }
            await _uow.SaveChangesAsync();
            return ToCartResponse(order);
        }

        public async Task<CartResponse> GetMyOpenCartAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            Guard.True(userId is not null, "User is not authenticated.");

            var order = await _uow.Orders.FindOpenOrderForUSerAsync(userId.Value, cancellationToken);

            if (order is null)
            {
                order = Order.CreateForUser(userId.Value, _clock.Utcnow, actorUserId: userId.Value);
                _uow.Orders.Add(order);
                await _uow.SaveChangesAsync();
            }
            return ToCartResponse(order);
        }

        private static CartResponse ToCartResponse(Order order)
        {
            var items = order.Items
                .Select(i => new CartItemResponse(i.ProductId, i.UnitPrice)).ToArray();
            return new CartResponse(
                order.Id,
                order.IsConfirmed,
                order.TotalPrice(),
                items);
        }
    }
}
