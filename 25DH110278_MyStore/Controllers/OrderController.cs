using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _25DH110278_MyStore.Models;
using _25DH110278_MyStore.Models.ViewModel;

namespace _25DH110278_MyStore.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Checkout()
        {
            var cart = CartService.GetCart(Session);
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new CheckoutVM();

            string currentUsername = Session["Username"] != null ? Session["Username"].ToString() : (User.Identity.IsAuthenticated ? User.Identity.Name : null);

            if (!string.IsNullOrEmpty(currentUsername))
            {
                var customer = db.Customers.FirstOrDefault(c => c.Username == currentUsername);
                if (customer != null)
                {
                    model.ReceiverName = customer.CustomerName;
                    model.ReceiverPhone = customer.CustomerPhone;
                    model.AddressDelivery = customer.CustomerAddress;
                }
            }

            ViewBag.Cart = cart;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model, string paymentMethod, string deliveryMethod)
        {
            var cart = CartService.GetCart(Session);
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                int customerId = 1;
                string currentUsername = Session["Username"] != null ? Session["Username"].ToString() : (User.Identity.IsAuthenticated ? User.Identity.Name : null);

                if (!string.IsNullOrEmpty(currentUsername))
                {
                    var customer = db.Customers.FirstOrDefault(c => c.Username == currentUsername);
                    if (customer != null)
                    {
                        customerId = customer.CustomerID;
                    }
                }
                else
                {
                    var existing = db.Customers.FirstOrDefault(c => c.CustomerPhone == model.ReceiverPhone);
                    if (existing != null)
                    {
                        customerId = existing.CustomerID;
                    }
                    else
                    {
                        var newCustomer = new Customer
                        {
                            CustomerName = model.ReceiverName,
                            CustomerPhone = model.ReceiverPhone,
                            CustomerAddress = model.AddressDelivery,
                            CustomerEmail = "guest@mystore.local"
                        };
                        db.Customers.Add(newCustomer);
                        db.SaveChanges();
                        customerId = newCustomer.CustomerID;
                    }
                }

                string status = "Chưa thanh toán";
                if (paymentMethod == "PayPal" || paymentMethod == "Online")
                {
                    status = "Đã thanh toán";
                }

                var order = new Order
                {
                    CustomerID = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.TotalAmount(),
                    PaymentStatus = status,
                    AddressDelivery = model.AddressDelivery
                };

                db.Orders.Add(order);
                db.SaveChanges();

                foreach (var item in cart.Items)
                {
                    var detail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    db.OrderDetails.Add(detail);
                }
                db.SaveChanges();

                CartService.ClearCart(Session);

                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            ViewBag.Cart = cart;
            return View(model);
        }

        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include(o => o.Customer)
                                 .Include(o => o.OrderDetails.Select(od => od.Product))
                                 .FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        public ActionResult MyOrder(string status, string searchTerm)
        {
            string currentUsername = Session["Username"] != null ? Session["Username"].ToString() : (User.Identity.IsAuthenticated ? User.Identity.Name : null);

            if (string.IsNullOrEmpty(currentUsername))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyOrder") });
            }

            var customer = db.Customers.FirstOrDefault(c => c.Username == currentUsername);
            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var query = db.Orders.Include(o => o.OrderDetails.Select(od => od.Product))
                                 .Where(o => o.CustomerID == customer.CustomerID);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.PaymentStatus == status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => o.OrderID.ToString().Contains(searchTerm) ||
                                         o.OrderDetails.Any(od => od.Product.ProductName.Contains(searchTerm)));
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();

            ViewBag.CurrentStatus = status;
            ViewBag.SearchTerm = searchTerm;

            return View(orders);
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
