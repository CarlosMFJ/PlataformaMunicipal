using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PlataformaMunicipal.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AsignacionSolicitud> AsignacionSolicituds { get; set; }

    public virtual DbSet<Ciudadano> Ciudadanos { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<EstadoSolicitud> EstadoSolicituds { get; set; }

    public virtual DbSet<Notificacion> Notificacions { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<ServicioMunicipal> ServicioMunicipals { get; set; }

    public virtual DbSet<SolicitudCiudadana> SolicitudCiudadanas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AsignacionSolicitud>(entity =>
        {
            entity.HasIndex(e => e.SolicitudId, "UX_Asignacion_Actual")
                .IsUnique()
                .HasFilter("([EsActual]=(1))");

            entity.Property(e => e.EsActual).HasDefaultValue(true, "DF_Asignacion_Actual");
            entity.Property(e => e.FechaAsignacion).HasDefaultValueSql("(sysdatetime())", "DF_Asignacion_Fecha");

            entity.HasOne(d => d.AsignadoPorUsuario).WithMany(p => p.AsignacionSolicituds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asignacion_Usuario");

            entity.HasOne(d => d.Departamento).WithMany(p => p.AsignacionSolicituds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asignacion_Departamento");

            entity.HasOne(d => d.Empleado).WithMany(p => p.AsignacionSolicituds).HasConstraintName("FK_Asignacion_Empleado");

            entity.HasOne(d => d.Solicitud).WithOne(p => p.AsignacionSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asignacion_Solicitud");
        });

        modelBuilder.Entity<Ciudadano>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Ciudadano_Activo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())", "DF_Ciudadano_Fecha");
            entity.Property(e => e.Sexo).IsFixedLength();
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Departamento_Activo");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Departamento_Fecha");

            entity.HasOne(d => d.JefeEmpleado).WithMany(p => p.Departamentos).HasConstraintName("FK_Departamento_Jefe");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Empleado_Activo");

            entity.HasOne(d => d.Departamento).WithMany(p => p.Empleados)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empleado_Departamento");
        });

        modelBuilder.Entity<EstadoSolicitud>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Estado_Activo");
            entity.Property(e => e.Color).IsFixedLength();
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.Property(e => e.Canal).HasDefaultValue("Sistema", "DF_Notif_Canal");
            entity.Property(e => e.FechaEnvio).HasDefaultValueSql("(sysdatetime())", "DF_Notif_Envio");
            entity.Property(e => e.Tipo).HasDefaultValue("Informativa", "DF_Notif_Tipo");

            entity.HasOne(d => d.Solicitud).WithMany(p => p.Notificacions).HasConstraintName("FK_Notificacion_Solicitud");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Notificacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificacion_Usuario");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Rol_Activo");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Rol_Fecha");
        });

        modelBuilder.Entity<ServicioMunicipal>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Servicio_Activo");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Servicio_Fecha");
            entity.Property(e => e.TiempoEstimadoDias).HasDefaultValue((short)5, "DF_Servicio_Dias");

            entity.HasOne(d => d.Departamento).WithMany(p => p.ServicioMunicipals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Servicio_Departamento");
        });

        modelBuilder.Entity<SolicitudCiudadana>(entity =>
        {
            entity.Property(e => e.Canal).HasDefaultValue("Web", "DF_Solicitud_Canal");
            entity.Property(e => e.FechaSolicitud).HasDefaultValueSql("(sysdatetime())", "DF_Solicitud_Fecha");
            entity.Property(e => e.Prioridad).HasDefaultValue((byte)2, "DF_Solicitud_Prioridad");

            entity.HasOne(d => d.Ciudadano).WithMany(p => p.SolicitudCiudadanas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitud_Ciudadano");

            entity.HasOne(d => d.Estado).WithMany(p => p.SolicitudCiudadanas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitud_Estado");

            entity.HasOne(d => d.Servicio).WithMany(p => p.SolicitudCiudadanas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitud_Servicio");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.CiudadanoId, "UX_Usuario_Ciudadano")
                .IsUnique()
                .HasFilter("([CiudadanoID] IS NOT NULL)");

            entity.HasIndex(e => e.EmpleadoId, "UX_Usuario_Empleado")
                .IsUnique()
                .HasFilter("([EmpleadoID] IS NOT NULL)");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Usuario_Activo");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Usuario_Fecha");

            entity.HasOne(d => d.Ciudadano).WithOne(p => p.Usuario).HasConstraintName("FK_Usuario_Ciudadano");

            entity.HasOne(d => d.Empleado).WithOne(p => p.Usuario).HasConstraintName("FK_Usuario_Empleado");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
