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
    
    public partial class Livres
    {
        public Nullable<int> Id { get; set; }
        public string Nom { get; set; }
        public string Auteur { get; set; }
        public string NbrPages { get; set; }
        public Nullable<decimal> Prix { get; set; }
        public Nullable<int> IdCoop { get; set; }
        public string CodeIdentification { get; set; }
    }
}
