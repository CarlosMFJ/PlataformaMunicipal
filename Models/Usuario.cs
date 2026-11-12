using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("Usuario")]
[Index("Correo", Name = "UQ_Usuario_Correo", IsUnique = true)]
[Index("NombreUsuario", Name = "UQ_Usuario_Nombre", IsUnique = true)]
public partial class Usuario
{
    [Key]
    [Column("UsuarioID")]
    public int UsuarioId { get; set; }

    [Column("RolID")]
    public int RolId { get; set; }

    [Column("EmpleadoID")]
    public int? EmpleadoId { get; set; }

    [Column("CiudadanoID")]
    public int? CiudadanoId { get; set; }

    [Required]
    [StringLength(50)]
    public string NombreUsuario { get; set; }

    [Required]
    [StringLength(120)]
    public string Correo { get; set; }

    [Required]
    [MaxLength(256)]
    public byte[] ContrasenaHash { get; set; }

    [Precision(0)]
    public DateTime? UltimoAcceso { get; set; }

    public byte IntentosFallidos { get; set; }

    public bool Bloqueado { get; set; }

    public bool Activo { get; set; }

    [Precision(0)]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("AsignadoPorUsuario")]
    public virtual ICollection<AsignacionSolicitud> AsignacionSolicituds { get; set; } = new List<AsignacionSolicitud>();

    [ForeignKey("CiudadanoId")]
    [InverseProperty("Usuario")]
    public virtual Ciudadano Ciudadano { get; set; }

    [ForeignKey("EmpleadoId")]
    [InverseProperty("Usuario")]
    public virtual Empleado Empleado { get; set; }

    [InverseProperty("Usuario")]
    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    [ForeignKey("RolId")]
    [InverseProperty("Usuarios")]
    public virtual Rol Rol { get; set; }
}
