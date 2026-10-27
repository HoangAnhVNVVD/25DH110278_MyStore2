﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH110278_MyStore.Models;

namespace _25DH110278_MyStore.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index()
        {
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalRevenue = db.Orders.Any() ? (db.Orders.Sum(o => (decimal?)o.TotalAmount) ?? 0) : 0;

            var categoryStats = db.Categories
                .Select(c => new
                {
                    Name = c.CategoryName,
                    Count = c.Products.Count()
                })
                .ToList();

            ViewBag.CategoryStats = categoryStats;

            var recentOrders = db.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            ViewBag.RecentOrders = recentOrders;

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
