﻿using Google.Apis.Books.v1.Data;
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
using Twilio;


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
        // GET: /Livres/RecuperationLivre
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult RecuperationLivre(string search1, string search2, string search3)
        {

            var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
            List<LivreAVendre> list = new List<LivreAVendre>();
            if (string.IsNullOrEmpty(search1) && string.IsNullOrEmpty(search2) && string.IsNullOrEmpty(search3))
            {
                list = db.LivreAVendreSet.Where(i => i.Cooperative == user.coopid && (i.Acheteur != "" && i.Acheteur != null)).ToList();
                return View(list);

            }
            list = db.LivreAVendreSet.Where(i => i.Cooperative == user.coopid && i.Acheteur.Contains(search1) && i.CodeIdentification.Contains(search2) && i.Titre.Contains(search3) && (i.Acheteur != "" && i.Acheteur != null)).ToList();
            return View(list);


        }
        // get: /Livres/RecuperationLivre
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult RecuperationLivreConfirmation(int id)
        {
            LivreAVendre livre = db.LivreAVendreSet.Where(i => i.Id == id).First();
            var listeacheteur = livre.Acheteur;
            List<string> myList = listeacheteur.TrimEnd(';').Split(';').ToList();
            if (myList.Count == 1)
            {
                if (livre.Quantite != 1)
                {
                    
                    var user = dbContext.Users.Where(i => i.UserName == myList[0]);
                    livre.Acheteur = null;
                    livre.DateReservation = null;
                    livre.Quantite--;
                    db.Entry(livre).State = EntityState.Modified;
                   

                }
                else
                {
                    db.LivreAVendreSet.Remove(livre);
                }
                this.AddHistorique(livre, "Achat");
                ViewBag.Success = true;
                List<LivreAVendre> list = new List<LivreAVendre>();
                var userAdmin = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
                list = db.LivreAVendreSet.Where(i => i.Cooperative == userAdmin.coopid && (i.Acheteur != "" && i.Acheteur != null)).ToList();
                return View("RecuperationLivre", list);

            }
            else
            {
                List<LivreAVendre> list = new List<LivreAVendre>(myList.Count());
               
                string[] dates = livre.DateReservation.Split(';');
               
                int index = 0;
                foreach (string str in myList)
                {
                    LivreAVendre livrevendu = new LivreAVendre();
                    livrevendu.Etat = livre.Etat;
                    livrevendu.CodeIdentification = livre.CodeIdentification;
                    livrevendu.Titre = livre.Titre;
                    livrevendu.Prix = livre.Prix;
                    livrevendu.Cooperative = livre.Cooperative;
                    livrevendu.DateReservation = dates[index];
                    livrevendu.Quantite = 1;
                    livrevendu.Acheteur = str;
                    list.Add(livrevendu);
                    index++;
                }
               
                return View(list);
            }
           
        }

        [Authorize(Roles = "Gestionnaire")]
        public ActionResult RecuperationLivreConfirmationEtudiant(string etat,string code, string acheteur, string date)
        {
            var livre = db.LivreAVendreSet.Where(i => i.CodeIdentification == code && i.Etat == etat).First();
            if(livre != null)
            {
                LivreAVendre livreRecuperer = livre;
                List<string> myList = livreRecuperer.Acheteur.TrimEnd(';').Split(';').ToList();
                List<string> myListdate = livreRecuperer.DateReservation.TrimEnd(';').Split(';').ToList();
                int index = 0;
                int indexAcheteur = -1;
                foreach(string ache in myList)
                {
                    if (ache == acheteur)                   
                        indexAcheteur = index;
                 
                    index++;
                }
                if(indexAcheteur != -1)
                {
                    myList.RemoveAt(indexAcheteur);
                    myListdate.RemoveAt(indexAcheteur);
                    livreRecuperer.Acheteur = "";
                    livreRecuperer.DateReservation = "";
                    foreach(string ache in myList)
                    {
                        livreRecuperer.Acheteur += ache + ";";
                        
                    }
                    foreach(string dateres in myListdate)
                    {
                        livreRecuperer.DateReservation += dateres + ";";
                    }
                    HistoriqueAchat achat = new HistoriqueAchat();
                    achat.Acheteur = acheteur;
                    achat.dateRecuperation = DateTime.Now.ToShortDateString();
                    achat.CodeIdentification = livreRecuperer.CodeIdentification;
                    achat.Etat = livreRecuperer.Etat;
                    achat.Cooperative = livreRecuperer.Cooperative.ToString();
                    achat.Prix = livreRecuperer.Prix;
                    if (db.HistoriqueAchat.Count() != 0)
                        achat.Id = db.HistoriqueAchat.Max(i => i.Id) + 1;
                    else
                        achat.Id = 1;
                    db.HistoriqueAchat.Add(achat);
                    livreRecuperer.Quantite--;
                    db.Entry(livreRecuperer).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.Success = true;
                }
                
            }
            List<LivreAVendre> list = new List<LivreAVendre>();
            var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
            list = db.LivreAVendreSet.Where(i => i.Cooperative == user.coopid && (i.Acheteur != "" && i.Acheteur != null)).ToList();
            return View("RecuperationLivre",list);
        }
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult ExpedierLivre(string search1, string search2, string search3)
        {

            var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
            List<LivreAVendre> list = new List<LivreAVendre>();
            if (string.IsNullOrEmpty(search1) && string.IsNullOrEmpty(search2) && string.IsNullOrEmpty(search3))
            {
                List<LivreAVendre> nouveau = new List<LivreAVendre>();
                list = db.LivreAVendreSet.Where(i => i.Cooperative == user.coopid && (i.Acheteur != "" && i.Acheteur != null)).ToList();
                int count = list.Count();
                for(int i = 0;i< count;i++)
                {
                    List<string> myList = list[i].Acheteur.TrimEnd(';').Split(';').ToList();
                    if(myList.Count != 1)
                    {
                            string[] dates = list[i].DateReservation.Split(';');

                            int index = 0;
                            foreach (string str in myList)
                            {
                                var acheteur = dbContext.Users.Where(t => t.UserName == str).FirstOrDefault();
                                if (acheteur.coopid != user.coopid)
                                {
                                    LivreAVendre livrevendu = new LivreAVendre();
                                    livrevendu.Etat = list[i].Etat;
                                    livrevendu.CodeIdentification = list[i].CodeIdentification;
                                    livrevendu.Titre = list[i].Titre;
                                    livrevendu.Prix = list[i].Prix;
                                    livrevendu.Id = list[i].Id;
                                    livrevendu.Auteur = list[i].Auteur;
                                    livrevendu.Proprietaire = list[i].Proprietaire;
                                    livrevendu.Cooperative = list[i].Cooperative;
                                    livrevendu.DateReservation = dates[index];
                                    livrevendu.Quantite = 1;
                                    livrevendu.Acheteur = str;
                                    nouveau.Add(livrevendu);
                                }
                                index++;
                            }
                        }
                    else
                    {
                        string Nom = myList[0];
                        var acheteur = dbContext.Users.Where(t => t.UserName == Nom).First();
                        if(acheteur.coopid != list[i].Cooperative)
                        {
                            nouveau.Add(list[i]);
                        }
                    }
                   
                }
               
              
                return View(nouveau);

            }
            list = db.LivreAVendreSet.Where(i => i.Cooperative == user.coopid && i.Acheteur.Contains(search1) && i.CodeIdentification.Contains(search2) && i.Titre.Contains(search3) && (i.Acheteur != "" && i.Acheteur != null)).ToList();
            return View(list);


        }
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult ExpedierLivreConfirm(int id)
        {

            return View();


        }
        [Authorize(Roles ="Gestionnaire")]
        public ActionResult ExpedierLivreChoix(string etat, string code, string acheteur, string date)
        {
            string nomAcheteur = acheteur.TrimEnd(';');
            var user = dbContext.Users.Where(i => i.UserName == nomAcheteur).First();
            var livre = db.LivreAVendreSet.Where(i => i.CodeIdentification == code && i.Etat == etat && (i.Acheteur != "" && i.Acheteur != null)).First();
            if (livre != null)
            {
                LivreAVendre livreRecuperer = livre;
                List<string> myList = livreRecuperer.Acheteur.TrimEnd(';').Split(';').ToList();
                List<string> myListdate = livreRecuperer.DateReservation.TrimEnd(';').Split(';').ToList();
                int index = 0;
                int indexAcheteur = -1;
                foreach (string ache in myList)
                {
                    if (ache == acheteur.TrimEnd(';'))
                        indexAcheteur = index;

                    index++;
                }
                if (indexAcheteur != -1)
                {
                    myList.RemoveAt(indexAcheteur);
                    myListdate.RemoveAt(indexAcheteur);
                    livreRecuperer.Acheteur = "";
                    livreRecuperer.DateReservation = "";
                    foreach (string ache in myList)
                    {
                        livreRecuperer.Acheteur += ache + ";";

                    }
                    foreach (string dateres in myListdate)
                    {
                        livreRecuperer.DateReservation += dateres + ";";
                    }
                    Expedier Transfert = new Expedier();
                    Transfert.NomEtudiant = acheteur;
                    Transfert.CodeIdentification = livreRecuperer.CodeIdentification;
                    Transfert.Etat = livreRecuperer.Etat;
                    Transfert.CooperativeProvenance = livreRecuperer.Cooperative.ToString();
                    Transfert.CooperativeEtudiant = user.coopid.ToString();
                    Transfert.Prix = livreRecuperer.Prix;
                    if (db.Expedier.Count() != 0)
                        Transfert.Id = db.Expedier.Max(i => i.Id) + 1;
                    else
                        Transfert.Id = 1;
                    db.Expedier.Add(Transfert);
                    if(livreRecuperer.Acheteur == "" && livreRecuperer.Quantite == 1)
                    {
                        db.Entry(livreRecuperer).State = EntityState.Deleted;
                    }
                    else
                    {
                        livreRecuperer.Quantite--;
                        db.Entry(livreRecuperer).State = EntityState.Modified;
                    }
                   
                    
                    db.SaveChanges();
                    ViewBag.Success = true;
                }

            }
            List<LivreAVendre> listNonNull = new List<LivreAVendre>();
            return View("ExpedierLivre", listNonNull);
        }
 
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult LivraisonLivre(string etat,string code ,string acheteur,string coop, string prix)
        {
            List<Expedier> list = new List<Expedier>();
            var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
            if (acheteur == null)
            {
                
                list = db.Expedier.Where(i => i.CooperativeEtudiant == user.coopid.ToString()).ToList();
            }
            else
            {
                int coopId = int.Parse(coop);
                LivreAVendre livre = db.LivreAVendreSet.Where(i => i.Etat == etat && i.CodeIdentification == code && i.Cooperative == coopId).FirstOrDefault();
                if(livre == null)
                {
                    LivreAVendre livrerecu = new LivreAVendre();
                    Livres livreInfo = db.Livres.Where(i => i.CodeIdentification == code).First();
                    livrerecu.Acheteur = acheteur+";";
                    livrerecu.Etat = etat;
                    livrerecu.CodeIdentification = code;
                    livrerecu.Auteur = livreInfo.Auteur;
                    livrerecu.Cooperative = user.coopid;
                    livrerecu.DateReservation = (DateTime.Now.ToShortDateString() + ";");
                    prix = prix.Replace('.', ',');
                    livrerecu.Prix = decimal.Parse(prix);
                    livrerecu.Quantite = 1;
                    livrerecu.Titre = livreInfo.Nom;

                    db.LivreAVendreSet.Add(livrerecu);
                    db.SaveChanges();

                }
                else
                {
                    livre.Acheteur += (User.Identity.Name + ";");
                    livre.DateReservation += (DateTime.Now.ToShortDateString() + ";");
                    livre.Quantite++;
                    db.Entry(livre).State = EntityState.Modified;

                    db.SaveChanges();
                }
            }
            
            return View(list);
        }
        // get: /Livres/RecuperationLivre
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult AnnulerLivre(int id)
        {
            LivreAVendre livre = db.LivreAVendreSet.Where(i => i.Id == id).First();
            var listeacheteur = livre.Acheteur;
            List<string> myList = listeacheteur.TrimEnd(';').Split(';').ToList();
            if (myList.Count == 1)
            {
                    livre.Acheteur = null;
                    livre.DateReservation = null;
                    db.Entry(livre).State = EntityState.Modified;
                HistoriqueAchat achat = new HistoriqueAchat();
                achat.Acheteur = myList[0];
                achat.dateRecuperation = DateTime.Now.ToShortDateString();
                achat.CodeIdentification = livre.CodeIdentification;
                achat.Etat = livre.Etat;
                achat.Prix = livre.Prix;
                achat.TypeTransaction = "Remboursement";
                if (db.HistoriqueAchat.Count() != 0)
                    achat.Id = db.HistoriqueAchat.Max(i => i.Id) + 1;
                else
                    achat.Id = 1;
                db.HistoriqueAchat.Add(achat);
                db.SaveChanges();
                ViewBag.SuccessCancel = true;

            }
            else
            {
                List<LivreAVendre> list = new List<LivreAVendre>(myList.Count());

                string[] dates = livre.DateReservation.Split(';');

                int index = 0;
                foreach (string str in myList)
                {
                    LivreAVendre livrevendu = new LivreAVendre();
                    livrevendu.Etat = livre.Etat;
                    livrevendu.CodeIdentification = livre.CodeIdentification;
                    livrevendu.Titre = livre.Titre;
                    livrevendu.Prix = livre.Prix;
                    livrevendu.Cooperative = livre.Cooperative;
                    livrevendu.DateReservation = dates[index];
                    livrevendu.Quantite = 1;
                    livrevendu.Acheteur = str;
                    list.Add(livrevendu);
                    index++;
                }

                return View("RecuperationLivreConfirmation",list);
            }

            List<LivreAVendre> listNonNull = new List<LivreAVendre>();
            var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
            listNonNull = db.LivreAVendreSet.Where(i => i.Cooperative == user.coopid && (i.Acheteur != "" && i.Acheteur != null)).ToList();
            return View("RecuperationLivre", listNonNull);
        }
        [Authorize(Roles = "Gestionnaire")]
        public ActionResult AnnulerLivreConfirmation(string etat, string code, string acheteur, string date)
        {
            var livre = db.LivreAVendreSet.Where(i => i.CodeIdentification == code && i.Etat == etat).First();
            if (livre != null)
            {
                LivreAVendre livreRecuperer = livre;
                List<string> myList = livreRecuperer.Acheteur.TrimEnd(';').Split(';').ToList();
                List<string> myListdate = livreRecuperer.DateReservation.TrimEnd(';').Split(';').ToList();
                int index = 0;
                int indexAcheteur = -1;
                foreach (string ache in myList)
                {
                    if (ache == acheteur)
                        indexAcheteur = index;

                    index++;
                }
                if (indexAcheteur != -1)
                {
                    myList.RemoveAt(indexAcheteur);
                    myListdate.RemoveAt(indexAcheteur);
                    livreRecuperer.Acheteur = "";
                    livreRecuperer.DateReservation = "";
                    foreach (string ache in myList)
                    {
                        livreRecuperer.Acheteur += ache + ";";

                    }
                    foreach (string dateres in myListdate)
                    {
                        livreRecuperer.DateReservation += dateres + ";";
                    }
                    db.Entry(livreRecuperer).State = EntityState.Modified;
                   
                    HistoriqueAchat achat = new HistoriqueAchat();
                    achat.Acheteur = acheteur;
                    achat.dateRecuperation = DateTime.Now.ToShortDateString();
                    achat.CodeIdentification = livreRecuperer.CodeIdentification;
                    achat.Etat = livreRecuperer.Etat;
                    achat.Prix = livreRecuperer.Prix;
                    achat.TypeTransaction = "Remboursement";
                    if (db.HistoriqueAchat.Count() != 0)
                        achat.Id = db.HistoriqueAchat.Max(i => i.Id) + 1;
                    else
                        achat.Id = 1;
                    db.HistoriqueAchat.Add(achat);
                    db.SaveChanges();
                    ViewBag.SuccessCancel = true;
                }
            }
                    List<LivreAVendre> listNonNull = new List<LivreAVendre>();
            var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
            listNonNull = db.LivreAVendreSet.Where(i => i.Cooperative == user.coopid && (i.Acheteur != "" && i.Acheteur != null)).ToList();
            return View("RecuperationLivre", listNonNull);
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
                 if(list.Count() == 0)
                {
                  
                        ViewBag.Search = search1;
                    ViewBag.NotFound = true;
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

                    for (int j = 0; j < dates.Length; j++)
                    {
                        if (dates[j] != "")
                        {
                            string[] sousDateTemp = dates[j].Split('-');

                            DateTime dateTemp = new DateTime(int.Parse(sousDateTemp[0]), int.Parse(sousDateTemp[1]), int.Parse(sousDateTemp[2]));
                            DateTime now = DateTime.Now.AddDays(-2);
                            if (dateTemp >= now)
                            {
                                nbLivreReserver += 1;
                            }
                           


                            i.Quantite = indexQte - nbLivreReserver;
                            if (i.Quantite == 0)
                            {
                                index += ind + ";";
                            }
                        }
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
        public ActionResult ConfirmationNotification(string value)
        {
            if(value != null || value != "")
            {
                WebApplication2.Models.Notification UserNotif = new WebApplication2.Models.Notification();
                var user = dbContext.Users.Where(i => i.UserName == User.Identity.Name).First();
                WebApplication2.Models.Notification notiftest = db.Notification.Where(i => i.CodeIdentification == value && i.NomEtudiant == User.Identity.Name).FirstOrDefault();
                if(notiftest == null)
                {
                    UserNotif.CodeIdentification = value;
                    UserNotif.NomEtudiant = user.UserName;
                    UserNotif.Cooperative = user.coopid;
                    if(db.Notification.Count() != 0)
                        UserNotif.id = db.Notification.Max(i => i.id) + 1;
                    else
                        UserNotif.id = 1;
                    db.Notification.Add(UserNotif);
                    db.SaveChanges();
                    ViewBag.Success = true;
                }
                else
                {
                    ViewBag.Warning = true;
                }
               
            }
            List<LivreAVendre> list = new List<LivreAVendre>();
            return View("ReserverLivre", list);
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

                        if( output.VolumeInfo.Authors != null)
                        {
                            search.Auteur = output.VolumeInfo.Authors[0];
                            search.NbrPages = output.VolumeInfo.PageCount.ToString();
                            search.Nom = output.VolumeInfo.Title;
                            ViewBag.Code = livres.CodeIdentification;
                            return RedirectToAction("EditGoogle", "Livres", livres = search);
                        }
                        else
                        {
                            ViewBag.Code = livres.CodeIdentification;
                            return RedirectToAction("EditGoogle", "Livres", livres = search);
                        }
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
                     if (db.LivreInventaire.Count() != 0)
                       livres.Id = db.LivreInventaire.Max(i => i.Id) + 1;
                     else
                         livres.Id = 1;

               
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
                var livretest = db.LivreInventaire.Where(i => i.NomEtudiant == livres.NomEtudiant && i.CodeIdentification == livres.CodeIdentification).FirstOrDefault();
                if (livretest != null)
                    livres.Id = livretest.Id;
                this.AjouterLivreAVendre(livres);
                this.RetirerLivreInventaire(livres);
                this.SendEmail(livres.NomEtudiant, "Votre livre a bien été ajouter au systeme de vente","Ajout livre");
                var user = dbContext.Users.Where(i => i.UserName == livres.NomEtudiant).FirstOrDefault();
                this.SendSMS(user.Telephone);
                List<WebApplication2.Models.Notification> listeNotif = db.Notification.Where(i => i.CodeIdentification == livres.CodeIdentification).ToList();
                for (int i = 0; i < listeNotif.Count(); i++)
                {
                    this.SendEmail(listeNotif[i].NomEtudiant, "Le livre que vous chercher a été rajouter au système", "L'attente est terminé");
                    db.Notification.Remove(listeNotif.ElementAt(i));
                    db.SaveChanges();
                }
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
        private void AddHistorique(LivreAVendre livre, String TypeTransaction)
        {
            HistoriqueAchat achat = new HistoriqueAchat();
            achat.Acheteur = livre.Acheteur;
            achat.dateRecuperation = DateTime.Now.ToShortDateString();
            achat.CodeIdentification = livre.CodeIdentification;
            achat.Etat = livre.Etat;
            achat.Prix = livre.Prix;
            achat.Cooperative = livre.Cooperative.ToString();
            achat.TypeTransaction = TypeTransaction;
            if (db.HistoriqueAchat.Count() != 0)
                achat.Id = db.HistoriqueAchat.Max(i => i.Id) + 1;
            else
                achat.Id = 1;
            db.HistoriqueAchat.Add(achat);
            db.SaveChanges();
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
        
        public void SendEmail(string user, string message, string objet)
        {
            
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("log210groupe5@gmail.com", "7898520789");

            MailMessage mm = new MailMessage("donotreply@domain.com", user, "Notification LivrEts: "+objet, message);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }

        public void SendSMS(string userPhone)
        {
            string Twilio_Account_SID = "ACa6fe6b5607b7f1f3be8b5158d30d9dee";
            string Twilio_Auth_Token = "ebf640051bfb7acc88f3b65cbc4d996f";
            string From_Number = "5147001808";
            TwilioRestClient twilioClient = new TwilioRestClient(Twilio_Account_SID, Twilio_Auth_Token);
            twilioClient.SendMessage(From_Number, userPhone, "L'état de votre livre a bien été confirmé par le gestionnaire de la Coop.");
            System.Diagnostics.Debug.WriteLine("SMS envoyé à : " + userPhone);
        }

    }
}
