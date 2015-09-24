using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication2.Controllers
{
    public class LivreController : Controller
    {
        // GET: Livre
        public ActionResult Index()
        {
            return View();
        }

        // GET: Livre/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Livre/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Livre/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Livre/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Livre/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Livre/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Livre/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
