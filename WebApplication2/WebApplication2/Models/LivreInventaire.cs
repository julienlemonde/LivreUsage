//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplication2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public partial class LivreInventaire
    {
        public int Id { get; set; }
        public string CodeIdentification { get; set; }
        public string Etat { get; set; }
        public int Quantite { get; set; }
        public Nullable<int> Cooperative { get; set; }
        public Boolean ContinuerAjout { get; set; }
        public IEnumerable<etat> ValeurEtat = new List<etat>
        {
            new etat
            {
                Etatid = 0,
                name = "Comme neuf"},
            new etat
            {
                Etatid = 1,
                name = "Moyennement Abîmé"
            },
            new etat
            {
                Etatid = 2,
                name = "Très Abîmé"
            }
        };
        public int typeId { get; set; }
        
    }
    public class etat
    {
        public int Etatid { get; set; }
        public string name { get; set; }
    }
   
}
