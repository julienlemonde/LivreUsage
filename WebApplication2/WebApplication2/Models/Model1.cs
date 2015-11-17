namespace WebApplication2.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CoopModel : DbContext
    {
        public CoopModel()
            : base("name=CoopModel")
        {
        }

        public virtual DbSet<Coop> Coop { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coop>()
                .Property(e => e.Nom)
                .IsUnicode(false);
        }

        public System.Data.Entity.DbSet<WebApplication2.Models.LivreAVendre> LivreAVendres { get; set; }
    }
}
