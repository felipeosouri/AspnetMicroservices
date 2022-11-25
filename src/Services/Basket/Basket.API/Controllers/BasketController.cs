using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IMapper _mapper;

        public BasketController(IBasketRepository repository, DiscountGrpcService discountGrpcService, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCar), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCar>> GetBasket(string userName)
        {
            var basket = await _repository.GetBasket(userName);
            return Ok(basket ?? new ShoppingCar(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCar), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCar>> UpdateBasket([FromBody] ShoppingCar basket)
        {
            foreach (var item in basket.Items)
            {
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }

            return Ok(await _repository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> DeleteBasket(string userName)
        {
            await _repository.DeleteBasket(userName);
            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            //get exisiting basket with total price
            var basket = await _repository.GetBasket(basketCheckout.UserName);
            if (basket is null)
            {
                return BadRequest();
            }

            //Create basketCheckoutEvent -- Set TotalPrice on BasketCheckout eventMessage

            //send checkout event to rabbitmq
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);


            //remove the basket
            await _repository.DeleteBasket(basket.UserName);

            return Accepted();
        }
    }
}
