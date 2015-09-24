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
    public class LivresController : Controller
    {
        private Cooperative db = new Cooperative();

        // GET: /Livres/
        public ActionResult Index()
        {
            return View(db.Livres.ToList());
        }

        // GET: /Livres/Details/5
        public ActionResult Details(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterLivreModel livres = new MasterLivreModel();
            livres.livres = db.Livres.Find(id);
            if (livres.livres == null)
            {
                return HttpNotFound();
            }
            return View(livres);
        }

        // GET: /Livres/Create
        [Authorize]
        public ActionResult Search()
        {
            return View();
        }

        // POST: /Livres/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Search([Bind(Include="CodeIdentification")] Livres livres)
        {
            if (ModelState.IsValid)
            {
                 Livres result = db.Livres.Find(livres.CodeIdentification);

                 if (result != null)
                 {
                     
                     return RedirectToAction("Details/"+result.CodeIdentification,"Livres");
                 }
                 else
                 {
                     return RedirectToAction("Create", "Livres");
                 }
            }

            return View(livres);
        }
        // GET: /Livres/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Livres/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nom,Auteur,NbrPages,Prix,Etat,IdCoop,CodeIdentification")] Livres livres)
        {
            if (ModelState.IsValid)
            {
                Livres result = db.Livres.Find(livres.CodeIdentification);

                if (result != null)
                {

                    return RedirectToAction("Details/" + livres.Id, "Livres");
                }
                else
                {
                    db.Livres.Add(livres);
                    db.SaveChanges();
                }
            }

            return View(livres);
        }

        // GET: /Livres/Edit/5
        public ActionResult Edit(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterLivreModel livres = new MasterLivreModel();
            livres.livres = db.Livres.Find(id);
            if (livres.livres == null)
            {
                return HttpNotFound();
            }
            return View(livres);
        }

        // POST: /Livres/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,Nom,Auteur,NbrPages,Prix,IdCoop,CodeIdentification")] MasterLivreModel livres)
        {
            if (ModelState.IsValid)
            {
                db.Entry(livres.livres).State = EntityState.Modified;
                db.LivreInventaire.Add(livres.livreinventaire);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(livres);
        }

        // GET: /Livres/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Livres livres = db.Livres.Find(id);
            if (livres == null)
            {
                return HttpNotFound();
            }
            return View(livres);
        }

        // POST: /Livres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Livres livres = db.Livres.Find(id);
            db.Livres.Remove(livres);
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
