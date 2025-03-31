namespace BlazorAppHuellero.Services.DTOs.Horarios
{
    public class HorariosDTO
    {
        public int Id { get; set; }
        public int IdConcepto { get; set; }
        public string CodigoConcepto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public List<ListaDias> DiasLaborales { get; set; }
    }

    public class CriterioHorariosDTO
    {
        public string Descripcion { get; set; }
    }

    public class ListaDias
    {
        public int Dia { get; set; }
    }

    public class HorariosUsuariosDTO
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdHorario { get; set; }
        public string? DescripcionUsuario { get; set; }
        public string? DescripcionHorario { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraFin { get; set; }

    }

    public class AsignarHorarioDTO
    {
        public int? Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdHorario { get; set; }

    }

    public class ConceptoHorario
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    }

    public class CriterioConceptoHorario
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    }
}
