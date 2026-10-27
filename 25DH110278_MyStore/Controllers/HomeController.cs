    using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using _25DH110278_MyStore.Models;
using _25DH110278_MyStore.Models.ViewModel;

namespace _25DH110278_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, int? categoryId, int? page)
        {
            var featuredQuery = db.Products.Include(p => p.Category)
                                           .OrderByDescending(p => p.OrderDetails.Count)
                                           .ThenByDescending(p => p.ProductID)
                                           .Take(10)
                                           .ToList();

            var newQuery = db.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                newQuery = newQuery.Where(p => p.CategoryID == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                newQuery = newQuery.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDescription.Contains(searchTerm) ||
                                               p.Category.CategoryName.Contains(searchTerm));
            }

            var newProductsList = newQuery.OrderByDescending(p => p.ProductID).Take(20).ToList();

            int pageSize = 6;
            int pageNumber = page ?? 1;

            var model = new HomeProductVM
            {
                FeaturedProducts = featuredQuery,
                NewProducts = newProductsList.ToPagedList(pageNumber, pageSize),
                SearchTerm = searchTerm,
                CategoryID = categoryId,
                Categories = db.Categories.ToList()
            };

            return View(model);
        }

        public ActionResult ProductList(int? categoryId, string searchTerm, int? page)
        {
            var query = db.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
                ViewBag.CurrentCategoryName = db.Categories.Where(c => c.CategoryID == categoryId.Value).Select(c => c.CategoryName).FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.ProductName.Contains(searchTerm) ||
                                         p.ProductDescription.Contains(searchTerm) ||
                                         p.Category.CategoryName.Contains(searchTerm));
            }

            query = query.OrderByDescending(p => p.ProductID);

            int pageSize = 8;
            int pageNumber = page ?? 1;

            ViewBag.CategoryID = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Categories = db.Categories.ToList();

            return View(query.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ProductDetail(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var related = db.Products
                .Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID)
                .OrderByDescending(p => p.ProductID)
                .Take(8)
                .ToList();

            var topDeals = db.Products
                .Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID)
                .OrderByDescending(p => p.OrderDetails.Count)
                .Take(8)
                .ToList();

            var model = new ProductDetailVM
            {
                Product = product,
                RelatedProducts = related,
                TopProducts = topDeals,
                Quantity = quantity ?? 1
            };

            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
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
