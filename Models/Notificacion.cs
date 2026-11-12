using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("Notificacion")]
[Index("UsuarioId", "Leida", "FechaEnvio", Name = "IX_Notificacion_Usuario", IsDescending = new[] { false, false, true })]
public partial class Notificacion
{
    [Key]
    [Column("NotificacionID")]
    public int NotificacionId { get; set; }

    [Column("UsuarioID")]
    public int UsuarioId { get; set; }

    [Column("SolicitudID")]
    public int? SolicitudId { get; set; }

    [Required]
    [StringLength(120)]
    public string Titulo { get; set; }

    [Required]
    [StringLength(500)]
    public string Mensaje { get; set; }

    [Required]
    [StringLength(20)]
    public string Tipo { get; set; }

    [Required]
    [StringLength(20)]
    public string Canal { get; set; }

    public bool Leida { get; set; }

    [Precision(0)]
    public DateTime FechaEnvio { get; set; }

    [Precision(0)]
    public DateTime? FechaLectura { get; set; }

    [ForeignKey("SolicitudId")]
    [InverseProperty("Notificacions")]
    public virtual SolicitudCiudadana Solicitud { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("Notificacions")]
    public virtual Usuario Usuario { get; set; }
}
