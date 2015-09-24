namespace WebApplication2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Coop")]
    public partial class Coop
    {
        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
          public int Id { get; set; }
        [Key]
        [StringLength(50)]     
        public string Nom { get; set; }

        [StringLength(50)]
        public string Adresse { get; set; }

        [StringLength(50)]
        public string NomGestionnaire { get; set; }
    }
    
}
