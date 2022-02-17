using Basket.API.Entities;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public interface IBasketRepository
    {
        Task<ShoppingCar> GetBasket(string userName);
        Task<ShoppingCar> UpdateBasket(ShoppingCar basket);
        Task DeleteBasket(string userName);
    }
}
