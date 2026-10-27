using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH110278_MyStore.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
        public decimal TotalPrice
        {
            get { return UnitPrice * Quantity; }
        }
    }

    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items
        {
            get { return items; }
        }

        public List<IGrouping<string, CartItem>> GroupedItems
        {
            get
            {
                return items.GroupBy(i => !string.IsNullOrEmpty(i.Category) ? i.Category : "Khác").ToList();
            }
        }

        public IPagedList<Product> SimilarProducts { get; set; }

        public void AddItem(Product product, int quantity = 1)
        {
            var item = items.FirstOrDefault(i => i.ProductID == product.ProductID);
            if (item == null)
            {
                items.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = product.ProductImage,
                    UnitPrice = product.ProductPrice,
                    Quantity = quantity,
                    Category = product.Category != null ? product.Category.CategoryName : ""
                });
            }
            else
            {
                item.Quantity += quantity;
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    items.Remove(item);
                }
            }
        }

        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        public decimal TotalAmount()
        {
            return items.Sum(i => i.TotalPrice);
        }

        public int TotalQuantity()
        {
            return items.Sum(i => i.Quantity);
        }

        public void Clear()
        {
            items.Clear();
        }

        public void ClearCart()
        {
            items.Clear();
        }
    }

    public class CartService
    {
        private const string CartSessionKey = "Cart";

        public static Cart GetCart(HttpSessionStateBase session)
        {
            Cart cart = session[CartSessionKey] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                session[CartSessionKey] = cart;
            }
            return cart;
        }

        public static void ClearCart(HttpSessionStateBase session)
        {
            session[CartSessionKey] = null;
        }
    }
}
