using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("AsignacionSolicitud")]
[Index("DepartamentoId", "EsActual", Name = "IX_Asignacion_Departamento")]
[Index("EmpleadoId", Name = "IX_Asignacion_Empleado")]
public partial class AsignacionSolicitud
{
    [Key]
    [Column("AsignacionID")]
    public int AsignacionId { get; set; }

    [Column("SolicitudID")]
    public int SolicitudId { get; set; }

    [Column("DepartamentoID")]
    public int DepartamentoId { get; set; }

    [Column("EmpleadoID")]
    public int? EmpleadoId { get; set; }

    [Column("AsignadoPorUsuarioID")]
    public int AsignadoPorUsuarioId { get; set; }

    [Precision(0)]
    public DateTime FechaAsignacion { get; set; }

    [Precision(0)]
    public DateTime? FechaLimite { get; set; }

    [Precision(0)]
    public DateTime? FechaCompletado { get; set; }

    [StringLength(500)]
    public string Comentario { get; set; }

    public bool EsActual { get; set; }

    [ForeignKey("AsignadoPorUsuarioId")]
    [InverseProperty("AsignacionSolicituds")]
    public virtual Usuario AsignadoPorUsuario { get; set; }

    [ForeignKey("DepartamentoId")]
    [InverseProperty("AsignacionSolicituds")]
    public virtual Departamento Departamento { get; set; }

    [ForeignKey("EmpleadoId")]
    [InverseProperty("AsignacionSolicituds")]
    public virtual Empleado Empleado { get; set; }

    [ForeignKey("SolicitudId")]
    [InverseProperty("AsignacionSolicitud")]
    public virtual SolicitudCiudadana Solicitud { get; set; }
}
