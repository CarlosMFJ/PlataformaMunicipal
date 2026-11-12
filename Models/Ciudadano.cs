using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

[Table("Ciudadano")]
[Index("Cedula", Name = "UQ_Ciudadano_Cedula", IsUnique = true)]
public partial class Ciudadano
{
    [Key]
    [Column("CiudadanoID")]
    public int CiudadanoId { get; set; }

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

    public DateOnly? FechaNacimiento { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string Sexo { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Telefono { get; set; }

    [StringLength(120)]
    public string Correo { get; set; }

    [StringLength(250)]
    public string Direccion { get; set; }

    [StringLength(100)]
    public string Sector { get; set; }

    [StringLength(100)]
    public string Municipio { get; set; }

    public bool Activo { get; set; }

    [Precision(0)]
    public DateTime FechaRegistro { get; set; }

    [InverseProperty("Ciudadano")]
    public virtual ICollection<SolicitudCiudadana> SolicitudCiudadanas { get; set; } = new List<SolicitudCiudadana>();

    [InverseProperty("Ciudadano")]
    public virtual Usuario Usuario { get; set; }
}
