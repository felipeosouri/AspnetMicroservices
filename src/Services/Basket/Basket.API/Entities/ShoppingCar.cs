using System.Collections.Generic;

namespace Basket.API.Entities
{
    public class ShoppingCar
    {
        public string UserName { get; set; }
        public List<ShoppingCarItem> Items { get; set; } = new List<ShoppingCarItem>();
        public ShoppingCar()
        {

        }
        public ShoppingCar(string userName)
        {
            UserName = userName;
        }

        public decimal TotalPrice
        {
            get
            {
                decimal totalPrice = 0;
                foreach (var item in Items)
                {
                    totalPrice += item.Price * item.Quantity;
                }
                return totalPrice;
            }
        }
    }
}
