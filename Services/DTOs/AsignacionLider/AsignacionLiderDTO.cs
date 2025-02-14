namespace BlazorAppHuellero.Services.DTOs.AsignacionLider
{
    public class AsignacionLiderDTO
    {
        public int? Id { get; set; }
        public int? IdLider { get; set; }
        public int? IdEmpleado { get; set; }
    }

    public class EmpleadosDTO
    {
        public int? Id { get; set; }
        public string? TipoIdentificacion { get; set; }
        public string? Identificacion { get; set; }
        public string? Nombre { get; set; }
    }

    public class EmpleadosBuscarDTO
    {
        public int? Id { get; set; }
        public string? Tipo_Identificacion { get; set; }
        public int? Identificacion { get; set; }
        public string? Nombre { get; set; }
    }

    public class CriterioEmpleadoDTO
    {
        public string Identificacion { get; set; }
        public string Nombre { get; set; }
    }

    public class EmpleadosLideresDTO
    {
        public int? Id { get; set; }
        public int? IdLider { get; set; }
        public int? IdEmpleado { get; set; }
        public string? NombreLider { get; set; }
        public string? NombreEmpleado { get; set; }
    }
}
