using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using _25DH110278_MyStore.Models;
using _25DH110278_MyStore.Models.ViewModel;

namespace _25DH110278_MyStore.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private Cart GetCart()
        {
            return CartService.GetCart(Session);
        }

        public ActionResult Index(int? page)
        {
            var cart = GetCart();
            int pageNumber = page ?? 1;
            cart.SimilarProducts = db.Products.OrderByDescending(p => p.ProductID).ToPagedList(pageNumber, 4);
            return View(cart);
        }

        public ActionResult AddItem(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                GetCart().AddItem(product, quantity);
            }
            return RedirectToAction("Index");
        }

        public ActionResult AddToCart(int id, int quantity = 1)
        {
            return AddItem(id, quantity);
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            GetCart().UpdateQuantity(id, quantity);
            return RedirectToAction("Index");
        }

        public ActionResult RemoveItem(int id)
        {
            GetCart().RemoveItem(id);
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromCart(int id)
        {
            return RemoveItem(id);
        }

        public ActionResult Clear()
        {
            GetCart().Clear();
            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            return Clear();
        }

        public ActionResult Checkout()
        {
            return RedirectToAction("Checkout", "Order");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
