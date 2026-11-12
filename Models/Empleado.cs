using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("Empleado")]
[Index("DepartamentoId", Name = "IX_Empleado_Departamento")]
[Index("Cedula", Name = "UQ_Empleado_Cedula", IsUnique = true)]
[Index("Correo", Name = "UQ_Empleado_Correo", IsUnique = true)]
public partial class Empleado
{
    [Key]
    [Column("EmpleadoID")]
    public int EmpleadoId { get; set; }

    [Column("DepartamentoID")]
    public int DepartamentoId { get; set; }

    [Required]
    [StringLength(13)]
    [Unicode(false)]
    public string Cedula { get; set; }

    [Required]
    [StringLength(80)]
    public string Nombres { get; set; }

    [Required]
    [StringLength(80)]
    public string Apellidos { get; set; }

    [Required]
    [StringLength(100)]
    public string Cargo { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Telefono { get; set; }

    [Required]
    [StringLength(120)]
    public string Correo { get; set; }

    public DateOnly FechaIngreso { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? Salario { get; set; }

    public bool Activo { get; set; }

    [InverseProperty("Empleado")]
    public virtual ICollection<AsignacionSolicitud> AsignacionSolicituds { get; set; } = new List<AsignacionSolicitud>();

    [ForeignKey("DepartamentoId")]
    [InverseProperty("Empleados")]
    public virtual Departamento Departamento { get; set; }

    [InverseProperty("JefeEmpleado")]
    public virtual ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();

    [InverseProperty("Empleado")]
    public virtual Usuario Usuario { get; set; }
}
