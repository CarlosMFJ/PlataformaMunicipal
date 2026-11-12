using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("EstadoSolicitud")]
[Index("Nombre", Name = "UQ_EstadoSolicitud_Nombre", IsUnique = true)]
public partial class EstadoSolicitud
{
    [Key]
    [Column("EstadoID")]
    public int EstadoId { get; set; }

    [Required]
    [StringLength(50)]
    public string Nombre { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    [StringLength(7)]
    [Unicode(false)]
    public string Color { get; set; }

    public byte Orden { get; set; }

    public bool EsInicial { get; set; }

    public bool EsFinal { get; set; }

    public bool Activo { get; set; }

    [InverseProperty("Estado")]
    public virtual ICollection<SolicitudCiudadana> SolicitudCiudadanas { get; set; } = new List<SolicitudCiudadana>();
}
