﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using _25DH110278_MyStore.Models;

namespace _25DH110278_MyStore.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, string statusFilter, int? page)
        {
            var orders = db.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o => o.Customer.CustomerName.Contains(searchTerm) ||
                                           o.AddressDelivery.Contains(searchTerm) ||
                                           o.OrderID.ToString().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                orders = orders.Where(o => o.PaymentStatus == statusFilter);
            }

            orders = orders.OrderByDescending(o => o.OrderDate);
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;
            return View(orders.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Include(o => o.Customer)
                                   .Include(o => o.OrderDetails.Select(od => od.Product))
                                   .FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Include(o => o.Customer).FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,CustomerID,OrderDate,TotalAmount,PaymentStatus,AddressDelivery")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        public ActionResult Delete(int? id)
        {
            return Details(id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            if (order != null)
            {
                var details = db.OrderDetails.Where(od => od.OrderID == id).ToList();
                foreach (var d in details)
                {
                    db.OrderDetails.Remove(d);
                }
                db.Orders.Remove(order);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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
