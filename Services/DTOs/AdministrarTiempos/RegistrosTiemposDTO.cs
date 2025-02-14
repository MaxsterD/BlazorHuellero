namespace BlazorAppHuellero.Services.DTOs.AdministrarTiempos
{
    public class RegistrosTiemposDTO
    {
        public string? IdUsuario { get; set; }
        public string? IdHorario { get; set; }
        public string? IdEntrada { get; set; }
        public string? IdSalida { get; set; }
        public string? Empleado { get; set; }
        public string? HoraEntrada { get; set; }
        public string? HoraSalida { get; set; }
        public string? Fecha { get; set; }
        public string? EstadoEntrada { get; set; }
        public string? EstadoSalida { get; set; }
             
    }

    public class RegistroDTO
    {
        public string? IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Fecha { get; set; }
        public string? Tipo { get; set; }
    }

    public class UsuarioBaseDTO
    {
        public string? IdUsuario { get; set; }
        public string? IdHorario { get; set; }
        public string? Fecha { get; set; }
        public string? HoraEntrada { get; set; }
        public string? EstadoEntrada { get; set; }
        public string? IdEntrada { get; set; }
        public string? HoraSalida { get; set; }
        public string? EstadoSalida { get; set; }
        public string? IdSalida { get; set; }

    }
}
