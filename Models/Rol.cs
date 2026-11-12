using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("Rol")]
[Index("Nombre", Name = "UQ_Rol_Nombre", IsUnique = true)]
public partial class Rol
{
    [Key]
    [Column("RolID")]
    public int RolId { get; set; }

    [Required]
    [StringLength(50)]
    public string Nombre { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    public byte NivelAcceso { get; set; }

    public bool PuedeAprobar { get; set; }

    public bool PuedeAsignar { get; set; }

    public bool Activo { get; set; }

    [Precision(0)]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("Rol")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
