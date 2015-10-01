﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        
        // GET: /Livres/Create
         [Authorize(Roles = "Gestionnaire")]
        public ActionResult RemiseLivre()
        {
            var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
            return View(db.LivreInventaire.Where(i=>i.Cooperative == user.coopid).ToList());
        }

        // GET: /Livres/
        [Authorize]
        public ActionResult Index(string code)
        {
            return View(db.Livres.ToList());
        }

        [Authorize(Roles = "Gestionnaire")]
        public ActionResult Confirmer(LivreInventaire livres)
        {
            if (ModelState.IsValid)
            {
                livres.Id = db.LivreInventaire.Where(i => i.NomEtudiant == livres.NomEtudiant && i.CodeIdentification == livres.CodeIdentification).FirstOrDefault().Id;
                this.AjouterLivreAVendre(livres);
                this.RetirerLivreInventaire(livres);
                return RedirectToAction("RemiseLivre", "Livres");
            }
            else
            {
                ModelState.AddModelError("", "Quelque chose ne va pas avec le model");
                return View(livres);
            }
        }
        // GET: /Livres/Details/5
        [Authorize]
        public ActionResult Details(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterLivreModel livres = new MasterLivreModel();
            livres.livres = db.Livres.Find(id);
            livres.Coop = new Coop();
            livres.Coop.Nom = db.Coop.Where(i => i.Id == livres.livres.IdCoop).FirstOrDefault().Nom;
            if (livres.livres == null)
            {
                return HttpNotFound();
            }
            return View(livres);
        }
        // GET: /Livres/Details/5
        [Authorize]
        public ActionResult DetailsConfirm(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterLivreModel livres = new MasterLivreModel();
            livres.livres = db.Livres.Find(id);
            livres.Coop = new Coop();
            livres.Coop.Nom = db.Coop.Where(i => i.Id == livres.livres.IdCoop).FirstOrDefault().Nom;
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
                    ViewBag.Code = livres.CodeIdentification;
                     return RedirectToAction("Create", "Livres",new { id = livres.CodeIdentification});
                 }
            }
            
            return View(livres);
        }
        // GET: /Livres/Create
        [Authorize]
        public ActionResult Create(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Livres livres = new Livres();
            livres.CodeIdentification = id;
            ViewBag.ChoixCoop = new SelectList(db.Coop.ToList(), "Id", "Nom");
            if (livres == null)
            {
                return HttpNotFound();
            }
            return View(livres);

           
        }

        // POST: /Livres/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nom,Auteur,NbrPages,Prix,Etat,IdCoop,CodeIdentification")] Livres livres)
        {
            if (ModelState.IsValid)
            {
                //livres.Id = int.Parse(livres.CodeIdentification.Replace("-", ""));
                Livres result = db.Livres.Where(i=> i.CodeIdentification == livres.CodeIdentification).FirstOrDefault();

                if (result != null)
                {

                    return RedirectToAction("Details/" + livres.CodeIdentification, "Livres");
                }
                else
                {
                    livres.Id = db.Livres.Count() + 1;
                    db.Livres.Add(livres);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("EditEtat", "Livres", new { id = livres.CodeIdentification });
        }

        // GET: /Livres/Edit/5
        [Authorize]
        public ActionResult Edit(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterLivreModel livres = new MasterLivreModel();
            livres.livres = new Livres();
            livres.livreinventaire = new LivreInventaire();
            livres.livres = db.Livres.Find(id);
            if (livres == null)
            {
                return HttpNotFound();
            }
            return View(livres.livres);
        }

        // POST: /Livres/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Nom,Auteur,NbrPages,Prix,IdCoop,CodeIdentification")] Livres livres)
        {
           
            if (ModelState.IsValid)
            {
                LivreInventaire LivreInv = new LivreInventaire();
                LivreInv = db.LivreInventaire.FirstOrDefault(i => i.CodeIdentification == livres.CodeIdentification);
                if(LivreInv == null)
                {
                    LivreInv = new LivreInventaire();
                    LivreInv.CodeIdentification = livres.CodeIdentification;
                    LivreInv.Cooperative = livres.IdCoop;
                    LivreInv.Quantite = 1;
                    LivreInv.Id = db.LivreInventaire.Count() + 1;
                }
               
               
                db.Entry(livres).State = EntityState.Modified;
               // db.LivreInventaire.Add(livres.livreinventaire);
                db.SaveChanges();
                return RedirectToAction("EditEtat","Livres", new { id = LivreInv.CodeIdentification });
            }
            return View(livres);
        }
        // GET: /Livres/Edit/5
        [Authorize]
        public ActionResult EditEtat(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterLivreModel livres = new MasterLivreModel();
            livres.livreinventaire = new LivreInventaire();

            
           
            

            livres.livres = new Livres();
            string code = db.Livres.Find(id).CodeIdentification;
            livres.livreinventaire.CodeIdentification = code;
            if (livres.livres == null)
            {
                return HttpNotFound();
            }
            return View(livres.livreinventaire);
        }
        // POST: /Livres/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult EditEtat([Bind(Include = "CodeIdentification,Etat,ContinuerAjout,typeID,ValeurEtat")] LivreInventaire livres)
        {
          
            if (ModelState.IsValid)
            {
                ApplicationDbContext user = new ApplicationDbContext();
                LivreInventaire LivreInv = new LivreInventaire();
                livres.Etat = livres.ValeurEtat.ElementAt(livres.typeId).name;
                livres.Cooperative = user.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().coopid;
                livres.NomEtudiant = User.Identity.Name.ToString();
                    livres.Id = db.LivreInventaire.Count() + 1;
                    livres.Quantite = 1;
                    db.LivreInventaire.Add(livres);
                    db.SaveChanges();
               
                
                
                
            }
            if (livres.ContinuerAjout == true)
                return RedirectToAction("Search", "Livres");
            else
                return RedirectToAction("Index","Home");
        }
        // GET: /Livres/EditEtatConfirm/5
        [Authorize]
        public ActionResult EditEtatConfirm(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterLivreModel livres = new MasterLivreModel();
            livres.livreinventaire = new LivreInventaire();





            livres.livres = new Livres();
            var livretrouve = db.LivreInventaire.Where(i=>i.CodeIdentification == id).First();
            livres.livreinventaire = livretrouve;
          
            if (livres.livres == null)
            {
                return HttpNotFound();
            }
            return View(livres.livreinventaire);
        }

        //Post EditEtatConfirm
        [HttpPost]
        [Authorize(Roles = "Gestionnaire")]
        [ValidateAntiForgeryToken]
        public ActionResult EditEtatConfirm([Bind(Include = "CodeIdentification,Etat,Cooperative,typeID,ValeurEtat,NomEtudiant")] LivreInventaire livres)
        {

            if (ModelState.IsValid)
            {
                livres.Id= db.LivreInventaire.Where(i => i.NomEtudiant == livres.NomEtudiant && i.CodeIdentification == livres.CodeIdentification).FirstOrDefault().Id;
                this.AjouterLivreAVendre(livres);
                this.RetirerLivreInventaire(livres);
                return RedirectToAction("RemiseLivre", "Livres");
            }
            else
            {
                ModelState.AddModelError("", "Quelque chose ne va pas avec le model");
                return View(livres);
            }
            
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
        private void AjouterLivreAVendre(LivreInventaire livres)
        {
            livres.Etat = livres.ValeurEtat.ElementAt(livres.typeId).name;
            var livrepret = db.LivreAVendreSet.Where(i => i.CodeIdentification == livres.CodeIdentification && i.Etat == livres.Etat && i.Etat == livres.Etat).FirstOrDefault();
            if (livrepret == null)
            {
                livrepret = new LivreAVendre();
                livrepret.CodeIdentification = livres.CodeIdentification;
                livrepret.Cooperative = livres.Cooperative;
                livrepret.Etat = livres.Etat;
                livrepret.Id = db.LivreAVendreSet.Count() + 1;
                livrepret.Quantite = 1;
                //Add a la db 
                db.LivreAVendreSet.Add(livrepret);
                db.SaveChanges();
            }
            else
            {
                    livrepret.Quantite++;
                    db.Entry(livrepret).State = EntityState.Modified;
                    db.SaveChanges();
               
            }

        }
        private void RetirerLivreInventaire(LivreInventaire livres)
        {
            if(livres != null)
            {
                var testlivre = db.LivreInventaire.Where(i => i.Id == livres.Id).FirstOrDefault();
                if (testlivre != null)
                {
                    db.Entry(testlivre).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            }
           

        }
    }
}
