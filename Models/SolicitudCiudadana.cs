using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("SolicitudCiudadana")]
[Index("CiudadanoId", Name = "IX_Solicitud_Ciudadano")]
[Index("EstadoId", "FechaSolicitud", Name = "IX_Solicitud_Estado_Fecha", IsDescending = new[] { false, true })]
[Index("ServicioId", Name = "IX_Solicitud_Servicio")]
[Index("NumeroSolicitud", Name = "UQ_Solicitud_Numero", IsUnique = true)]
public partial class SolicitudCiudadana
{
    [Key]
    [Column("SolicitudID")]
    public int SolicitudId { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string NumeroSolicitud { get; set; }

    [Column("CiudadanoID")]
    public int CiudadanoId { get; set; }

    [Column("ServicioID")]
    public int ServicioId { get; set; }

    [Column("EstadoID")]
    public int EstadoId { get; set; }

    [Required]
    [StringLength(150)]
    public string Asunto { get; set; }

    public string Descripcion { get; set; }

    public byte Prioridad { get; set; }

    [Required]
    [StringLength(20)]
    public string Canal { get; set; }

    [StringLength(250)]
    public string DireccionIncidente { get; set; }

    [Precision(0)]
    public DateTime FechaSolicitud { get; set; }

    [Precision(0)]
    public DateTime? FechaLimite { get; set; }

    [Precision(0)]
    public DateTime? FechaResolucion { get; set; }

    [StringLength(500)]
    public string Observaciones { get; set; }

    [InverseProperty("Solicitud")]
    public virtual AsignacionSolicitud AsignacionSolicitud { get; set; }

    [ForeignKey("CiudadanoId")]
    [InverseProperty("SolicitudCiudadanas")]
    public virtual Ciudadano Ciudadano { get; set; }

    [ForeignKey("EstadoId")]
    [InverseProperty("SolicitudCiudadanas")]
    public virtual EstadoSolicitud Estado { get; set; }

    [InverseProperty("Solicitud")]
    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    [ForeignKey("ServicioId")]
    [InverseProperty("SolicitudCiudadanas")]
    public virtual ServicioMunicipal Servicio { get; set; }
}
