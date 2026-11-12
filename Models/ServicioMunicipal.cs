using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("ServicioMunicipal")]
[Index("DepartamentoId", Name = "IX_Servicio_Departamento")]
[Index("Codigo", Name = "UQ_ServicioMunicipal_Codigo", IsUnique = true)]
public partial class ServicioMunicipal
{
    [Key]
    [Column("ServicioID")]
    public int ServicioId { get; set; }

    [Column("DepartamentoID")]
    public int DepartamentoId { get; set; }

    [Required]
    [StringLength(15)]
    [Unicode(false)]
    public string Codigo { get; set; }

    [Required]
    [StringLength(120)]
    public string Nombre { get; set; }

    [StringLength(500)]
    public string Descripcion { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Costo { get; set; }

    public short TiempoEstimadoDias { get; set; }

    public bool RequiereDocumentos { get; set; }

    public bool Activo { get; set; }

    [Precision(0)]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("DepartamentoId")]
    [InverseProperty("ServicioMunicipals")]
    public virtual Departamento Departamento { get; set; }

    [InverseProperty("Servicio")]
    public virtual ICollection<SolicitudCiudadana> SolicitudCiudadanas { get; set; } = new List<SolicitudCiudadana>();
}
