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
    public class CoopsController : Controller
    {
        private Cooperative db = new Cooperative();

        // GET: Coops
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult Index()
        {
            return View(db.Coop.ToList());
        }

        // GET: Coops/Details/5
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coop coop = db.Coop.Where(i => i.Id == id).FirstOrDefault();
            if (coop == null)
            {
                return HttpNotFound();
            }
            return View(coop);
        }

        // GET: Coops/Create
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Coops/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Gestionnaire")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nom,Adresse, NomGestionnaire")] Coop coop)
        {
            if (ModelState.IsValid)
            {
                Coop result = db.Coop.FirstOrDefault(i => i.Nom == coop.Nom);
       
                if (result == null)
                {
                    coop.NomGestionnaire = User.Identity.Name;
                    Coop testGestionnaire = db.Coop.Where(i => i.NomGestionnaire == User.Identity.Name).FirstOrDefault();
                    if(testGestionnaire == null)
                    {
                        coop.Id = db.Coop.Count() + 1;
                        db.Coop.Add(coop);
                        db.SaveChanges();
                        return RedirectToAction("Details/" + coop.Id, "Coops");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Vous êtes déjà associé à une coopérative");
                        return View(coop);
                    }
                    
                }
                else
                {
                    ModelState.AddModelError("", "La coopérative " + coop.Nom + " existe déjà");
                    return View(coop);
                }
                   




            }

            return View(coop);
        }


        // GET: Coops/Edit/5
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coop coop = db.Coop.Find(id);
            if (coop == null)
            {
                return HttpNotFound();
            }
            return View(coop);
        }

        // POST: Coops/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Gestionnaire")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nom")] Coop coop)
        {
            if (ModelState.IsValid)
            {
                db.Entry(coop).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(coop);
        }

        // GET: Coops/Delete/5
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Coop coop = db.Coop.Find(id);
            if (coop == null)
            {
                return HttpNotFound();
            }
            return View(coop);
        }

        // POST: Coops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult DeleteConfirmed(int id)
        {
            Coop coop = db.Coop.Find(id);
            db.Coop.Remove(coop);
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
