using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class MasterLivreModel
    {
        public Livres livres { get; set; }
        public LivreInventaire livreinventaire { get; set; }
        public Coop Coop { get; set; }
        public LivreAVendre livreVendre { get; set; }
    }
}