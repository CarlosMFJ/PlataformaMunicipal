using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("Departamento")]
[Index("Codigo", Name = "UQ_Departamento_Codigo", IsUnique = true)]
[Index("Nombre", Name = "UQ_Departamento_Nombre", IsUnique = true)]
public partial class Departamento
{
    [Key]
    [Column("DepartamentoID")]
    public int DepartamentoId { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string Codigo { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [StringLength(300)]
    public string Descripcion { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Telefono { get; set; }

    [StringLength(120)]
    public string Correo { get; set; }

    [StringLength(150)]
    public string Ubicacion { get; set; }

    [Column("JefeEmpleadoID")]
    public int? JefeEmpleadoId { get; set; }

    public bool Activo { get; set; }

    [Precision(0)]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("Departamento")]
    public virtual ICollection<AsignacionSolicitud> AsignacionSolicituds { get; set; } = new List<AsignacionSolicitud>();

    [InverseProperty("Departamento")]
    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    [ForeignKey("JefeEmpleadoId")]
    [InverseProperty("Departamentos")]
    public virtual Empleado JefeEmpleado { get; set; }

    [InverseProperty("Departamento")]
    public virtual ICollection<ServicioMunicipal> ServicioMunicipals { get; set; } = new List<ServicioMunicipal>();
}
