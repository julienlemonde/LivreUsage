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
    
    public partial class LivreAVendre
    {
        public int Id { get; set; }
        public Nullable<int> Quantite { get; set; }
        public Nullable<int> Cooperative { get; set; }
        public string CodeIdentification { get; set; }
        public string Etat { get; set; }
    }
}
