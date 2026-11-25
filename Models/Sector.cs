using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("Sector")]
[Index("Codigo", Name = "UQ_Sector_Codigo", IsUnique = true)]
[Index("Nombre", Name = "UQ_Sector_Nombre", IsUnique = true)]
public partial class Sector
{
    [Key]
    [Column("SectorID")]
    public int SectorId { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string Codigo { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [Required]
    [StringLength(100)]
    public string Municipio { get; set; }

    [StringLength(300)]
    public string Descripcion { get; set; }

    public bool Activo { get; set; }

    [Precision(0)]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("Sector")]
    public virtual ICollection<Ciudadano> Ciudadanos { get; set; } = new List<Ciudadano>();
}
