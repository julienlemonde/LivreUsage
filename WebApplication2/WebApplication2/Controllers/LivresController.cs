using Google.Apis.Books.v1.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class LivresController : Controller
    {
        private Cooperative db = new Cooperative();
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        private Livres search = new Livres();
        
        // GET: /Livres/Create
         [Authorize(Roles = "Gestionnaire")]
        public ActionResult RemiseLivre(string search1, string search2, string search3)
        {
           
            var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
            if (string.IsNullOrEmpty(search1) && string.IsNullOrEmpty(search2) && string.IsNullOrEmpty(search3))
             {
                 return View(db.LivreInventaire.Where(i => i.Cooperative == user.coopid).ToList());
                 
             }

            return View(db.LivreInventaire.Where(i => i.Cooperative == user.coopid && i.NomEtudiant.Contains(search1) && i.CodeIdentification.Contains(search2) && i.Titre.Contains(search3)).ToList());
                 
            
        }
        [Authorize]
        public ActionResult ConfirmationPaiement(int id)
         {
             LivreAVendre livreChoisi = db.LivreAVendreSet.Where(i => i.Id == id).First();
             livreChoisi.Acheteur += (User.Identity.Name + ";");
             livreChoisi.DateReservation += (DateTime.Now.ToShortDateString() + ";");
             db.Entry(livreChoisi).State = EntityState.Modified;
 
             db.SaveChanges();

            return  RedirectToAction("Index", "Home");
         }
         // GET: /Livres/Create
         [Authorize]
         public ActionResult ReserverLivre(string search1, string search2, string search3)
         {
             var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
             List<LivreAVendre> list = new List<LivreAVendre>();
             if (string.IsNullOrEmpty(search1) && string.IsNullOrEmpty(search2) && string.IsNullOrEmpty(search3))
             {
                 
                

             }
             else
             {
                 
                 list = db.LivreAVendreSet.Where(i => i.Cooperative == user.coopid && i.CodeIdentification.Contains(search1) && i.Auteur.Contains(search2) && i.Titre.Contains(search3)).ToList();
                 if(list.Count()==0)
                 {
                     list = db.LivreAVendreSet.Where(i => i.CodeIdentification.Contains(search1) && i.Auteur.Contains(search2) && i.Titre.Contains(search3)).ToList();
                 }
             }
             string index = "";
             int ind = 0;
               foreach(LivreAVendre i in list)
                 {
                     int? indexQte = i.Quantite;
                     if (i.DateReservation != null)
                     {
                         string[] dates = i.DateReservation.Split(';');
                         int nbLivreReserver = 0;
                         for (int j = 0; j < indexQte; j++)
                         {
                             string[] sousDateTemp = dates[j].Split('-');

                             DateTime dateTemp = new DateTime(int.Parse(sousDateTemp[0]), int.Parse(sousDateTemp[1]), int.Parse(sousDateTemp[2]));
                             DateTime now = DateTime.Now.AddDays(-2);
                             if (dateTemp >= now)
                             {
                                 nbLivreReserver += 1;
                             }
                             else
                             {
                                 nbLivreReserver -= 1;
                             }
                         }
                         i.Quantite = indexQte - nbLivreReserver;
                         if (i.Quantite == 0)
                         {
                             index += ind + ";";
                         }
                     }
                     ind++;
                 }
             if(index != "")
             {
                 string[] ASupprimer = index.Split(';');
                 foreach (string i in ASupprimer)
                 {
                     if(i != "")
                     list.RemoveAt(int.Parse(i));
                 }
             }
            

              return View(list);


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
        public ActionResult DetailsReservation(int id)
        {
            ApplicationDbContext user = new ApplicationDbContext();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           MasterLivreModel livres = new MasterLivreModel();
           livres.livreVendre = db.LivreAVendreSet.Where(i => i.Id == id).First();
            livres.Coop = new Coop();
           livres.Coop = db.Coop.Where(i => i.Id == livres.livreVendre.Cooperative).FirstOrDefault();

           NumberFormatInfo nfi = new NumberFormatInfo();
           nfi.NumberDecimalSeparator = ".";
           double prix = (double)livres.livreVendre.Prix;
           string ValeurPrix = prix.ToString(nfi);
           ViewBag.Prix = ValeurPrix;
           string ConfirmURL = "http://localhost:56740/Livres/ConfirmationPaiement/" + livres.livreVendre.Id;
           ViewBag.ConfirmURL = ConfirmURL;
            if(livres.Coop.Id != user.Users.Where(i => i.UserName == User.Identity.Name).First().coopid)
            {
                ModelState.AddModelError("","Le livre sélectionné se trouve dans une coopérative différente, il y aura un frais supplémentaire pour le transport");
                ViewBag.Frais = "5.00";
            }
            else
            {
                ViewBag.Frais = "0.00";
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
                Livres result =  db.Livres.Find(livres.CodeIdentification);
                livres.CodeIdentification = livres.CodeIdentification.Replace(" ", "");

                 if (result != null)
                 {
                    
                    return RedirectToAction("Details/"+result.CodeIdentification,"Livres");
                 }
                 else
                 {
                    string ISBN = livres.CodeIdentification.Replace("-", "");
                    var output = BookSearch.SearchISBN(ISBN);
                    if (output != null)
                    {
                        search.CodeIdentification = livres.CodeIdentification;
                        search.Auteur = output.VolumeInfo.Authors[0];
                        search.NbrPages = output.VolumeInfo.PageCount.ToString();
                        search.Nom = output.VolumeInfo.Title;
                        ViewBag.Code = livres.CodeIdentification;
                        return RedirectToAction("EditGoogle", "Livres", livres = search);
                      }
                else
                    {
                        ViewBag.Code = livres.CodeIdentification;
                        return RedirectToAction("Create", "Livres",new { id = livres.CodeIdentification});
                    }

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
        //Get EditGOOGLE
        [Authorize]
        public ActionResult EditGoogle(Livres livres)
        {
            ApplicationDbContext user = new ApplicationDbContext();
            if (livres == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (livres.IdCoop == null)
            {
                livres.IdCoop = user.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().coopid;
                return View(livres);
            }
            else
            {
                livres.Id = db.Livres.Count() + 1;
                db.Livres.Add(livres);
                db.SaveChanges();
                return RedirectToAction("EditEtat", "Livres", new { id = livres.CodeIdentification });
            }
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
            Livres recherchecode = db.Livres.Where(i => i.CodeIdentification == id).FirstOrDefault();
            if (recherchecode == null)
                livres.livreinventaire.CodeIdentification = id;
            else
                livres.livreinventaire.CodeIdentification = recherchecode.CodeIdentification;

            
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
                    livres.Titre = db.Livres.Where(i=>i.CodeIdentification == livres.CodeIdentification).FirstOrDefault().Nom;
                    livres.NomEtudiant = User.Identity.Name.ToString();
                    livres.Id = db.LivreInventaire.Max(i => i.Id) + 1;
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
        public ActionResult EditEtatConfirm(String id, String etudiant)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterLivreModel livres = new MasterLivreModel();
            livres.livreinventaire = new LivreInventaire();





            livres.livres = new Livres();
            var livretrouve = db.LivreInventaire.Where(i => i.CodeIdentification == id && i.NomEtudiant == etudiant).First();
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
                this.SendEmail(livres.NomEtudiant);
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
            Livres InfoLivre = db.Livres.Where(i => i.CodeIdentification == livres.CodeIdentification).First();
            if (livrepret == null || livrepret.Cooperative != livres.Cooperative)
            {
                livrepret = new LivreAVendre();
                livrepret.CodeIdentification = livres.CodeIdentification;
                livrepret.Cooperative = livres.Cooperative;
                livrepret.Etat = livres.Etat;
                livrepret.Titre = InfoLivre.Nom;
                livrepret.Id = db.LivreAVendreSet.Count() + 1;
                livrepret.Auteur = InfoLivre.Auteur;
                
                if(livres.Etat == "Comme Neuf")
                {
                    livrepret.Prix = ((InfoLivre.Prix)*(decimal)0.75);
                }
                else if (livres.Etat == "Moyennement Abîmé")
                {
                    livrepret.Prix = ((InfoLivre.Prix) * (decimal)0.50);

                }
                else
                {
                    livrepret.Prix = ((InfoLivre.Prix) * (decimal)0.25);
                }
                livrepret.Proprietaire += (livres.NomEtudiant + ";");
                livrepret.Quantite = 1;
                //Add a la db 
                db.LivreAVendreSet.Add(livrepret);
                db.SaveChanges();
            }
            else if(livrepret.Cooperative == livres.Cooperative)
            {
                    livrepret.Quantite++;
                    livrepret.Proprietaire += (livres.NomEtudiant + ";");
                    db.Entry(livrepret).State = EntityState.Modified;
                    db.SaveChanges();
               
            }
            

        }
        private void RetirerLivreInventaire(LivreInventaire livres)
        {
            if(livres != null)
            {
                var testlivre = db.LivreInventaire.Where(i => i.Id == livres.Id && i.NomEtudiant == livres.NomEtudiant).FirstOrDefault();
                if (testlivre != null)
                {
                    db.Entry(testlivre).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            }
           

        }
        
        public void SendEmail(string user)
        {
            
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("log210groupe5@gmail.com", "7898520789");

            MailMessage mm = new MailMessage("donotreply@domain.com", user, "Notification LivrEts", "Votre livre a bien été ajouté au système de vente");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }

    }
}
