using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using _25DH110278_MyStore.Models;
using _25DH110278_MyStore.Models.ViewModel;

namespace _25DH110278_MyStore.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại trên hệ thống.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "C"
                };
                db.Users.Add(user);

                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(customer);

                db.SaveChanges();

                Session["Username"] = user.Username;
                Session["UserRole"] = user.UserRole;
                Session["CustomerName"] = customer.CustomerName;

                FormsAuthentication.SetAuthCookie(user.Username, false);

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
                    if (customer != null)
                    {
                        Session["CustomerName"] = customer.CustomerName;
                    }
                    else
                    {
                        Session["CustomerName"] = user.Username;
                    }

                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    if (user.UserRole == "A" || user.UserRole == "Admin")
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ProfileInfo()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("ProfileInfo") });
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        new public ActionResult Profile()
        {
            return RedirectToAction("ProfileInfo");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(Customer customer)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                Session["CustomerName"] = customer.CustomerName;
                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
            }
            return RedirectToAction("ProfileInfo");
        }

        public ActionResult ChangePassword()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("ChangePassword") });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                string username = Session["Username"].ToString();
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.Password == model.OldPassword);
                if (user != null)
                {
                    user.Password = model.NewPassword;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("ProfileInfo");
                }
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
            }
            return View(model);
        }

        public ActionResult OrderHistory()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("OrderHistory") });
            }

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = db.Orders.Include(o => o.OrderDetails.Select(od => od.Product))
                                  .Where(o => o.CustomerID == customer.CustomerID)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToList();

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
