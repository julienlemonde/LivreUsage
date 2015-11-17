using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class ExpedierController : Controller
    {
        private CoopModel db = new CoopModel();

        // GET: LivreAVendres
        public ActionResult ExpedierLivre(int id)
        {
            return View(db.LivreAVendres.ToList());
        }

        // GET: LivreAVendres/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LivreAVendre livreAVendre = db.LivreAVendres.Find(id);
            if (livreAVendre == null)
            {
                return HttpNotFound();
            }
            return View(livreAVendre);
        }

        // GET: LivreAVendres/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LivreAVendres/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Quantite,Cooperative,CodeIdentification,Etat,Titre,Prix,Proprietaire,Auteur,Acheteur,DateReservation")] LivreAVendre livreAVendre)
        {
            if (ModelState.IsValid)
            {
                db.LivreAVendres.Add(livreAVendre);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(livreAVendre);
        }

        // GET: LivreAVendres/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LivreAVendre livreAVendre = db.LivreAVendres.Find(id);
            if (livreAVendre == null)
            {
                return HttpNotFound();
            }
            return View(livreAVendre);
        }

        // POST: LivreAVendres/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Quantite,Cooperative,CodeIdentification,Etat,Titre,Prix,Proprietaire,Auteur,Acheteur,DateReservation")] LivreAVendre livreAVendre)
        {
            if (ModelState.IsValid)
            {
                db.Entry(livreAVendre).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(livreAVendre);
        }

        // GET: LivreAVendres/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LivreAVendre livreAVendre = db.LivreAVendres.Find(id);
            if (livreAVendre == null)
            {
                return HttpNotFound();
            }
            return View(livreAVendre);
        }

        // POST: LivreAVendres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LivreAVendre livreAVendre = db.LivreAVendres.Find(id);
            db.LivreAVendres.Remove(livreAVendre);
            db.SaveChanges();
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
