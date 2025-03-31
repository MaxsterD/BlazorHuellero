using BlazorAppHuellero.Components.Modales;
using BlazorAppHuellero.CustomStyle;
using BlazorAppHuellero.Services.DTOs;
using BlazorAppHuellero.Services.DTOs.CreacionUsuario;
using BlazorAppHuellero.Services.DTOs.Horarios;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace BlazorAppHuellero.Pages.Horarios
{
    public partial class Horarios
    {
        private HorariosDTO horario = new HorariosDTO();
        private ConceptoHorario Concepto = new ConceptoHorario();
        private ConceptoHorario CriterioConcepto = new ConceptoHorario();
        private List<HorariosDTO> ListaHorarios { get; set; } = new List<HorariosDTO>();
        private Dictionary<int, bool> DiasSeleccionados = new()
        {
            { 1, false }, { 2, false }, { 3, false }, { 4, false },
            { 5, false }, { 6, false }, { 7, false }
        };
        private Dictionary<int, bool> DiasSeleccionadosEdit = new();

        public Placement Placement { get; set; } = Placement.Bottom;

        [Inject] HttpClient httpClient { get; set; }
        [Inject] AuthenticationStateProvider autenticacionProvider { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IConfiguration Configuration { get; set; }

        private string titleBarStyle = $"height:7%;background-color:{AtowerTheme.Default.PaletteLight.Primary}; color:white;";
        private string colorStyle = $"color:{AtowerTheme.Default.PaletteLight.Primary};";
        private MudTimePicker horaInicio;
        private MudTimePicker horaFin;


        private void CargarDiasSeleccionados(HorariosDTO horario)
        {
            // Inicializar el diccionario de días laborales
            DiasSeleccionadosEdit = Enumerable.Range(1, 7).ToDictionary(d => d, d => false);

            if (horario?.DiasLaborales != null)
            {
                foreach (var dia in horario.DiasLaborales)
                {
                    if (DiasSeleccionadosEdit.ContainsKey(dia.Dia))
                    {
                        DiasSeleccionadosEdit[dia.Dia] = true;
                    }
                }
            }

            
        }

        private void OnDayChangedEdit(HorariosDTO horario, int dia, bool isChecked)
        {
            if (horario.DiasLaborales == null)
            {
                horario.DiasLaborales = new List<ListaDias>();
            }

            if (isChecked)
            {
                if (!horario.DiasLaborales.Any(d => d.Dia == dia))
                {
                    horario.DiasLaborales.Add(new ListaDias { Dia = dia });
                }
            }
            else
            {
                horario.DiasLaborales.RemoveAll(d => d.Dia == dia);
            }

            // Actualizar el diccionario para reflejar el cambio en la UI
            DiasSeleccionadosEdit[dia] = isChecked;
           
        }

        private string GetDayLabel(int dayNumber)
        {
            return dayNumber switch
            {
                1 => "Domingo",
                2 => "Lunes",
                3 => "Martes",
                4 => "Miércoles",
                5 => "Jueves",
                6 => "Viernes",
                7 => "Sábado"
            };
        }



        private void UpdateDiasLaborales()
        {
            Console.WriteLine("Prueba2");

            horario.DiasLaborales = DiasSeleccionados
                .Where(d => d.Value)  // Take only checked days
                .Select(d => new ListaDias { Dia = d.Key })
                .ToList();

        }

        private void OnDayChanged(int day, bool isChecked)
        {
            DiasSeleccionados[day] = isChecked;
            UpdateDiasLaborales();
        }



        private async Task Guardar()
        {
            if (horario == null)
            {
                Snackbar.Add("Por favor, introduce los datos del Horario.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (Concepto == null)
            {
                Snackbar.Add("Por favor, introduce los datos del Concepto.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }else if (string.IsNullOrEmpty(Concepto.Descripcion))
            {
                Snackbar.Add("Por favor, seleccone un Concepto.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (string.IsNullOrEmpty(horario.Descripcion))
            {
                Snackbar.Add("Por favor, introduce una Descripción.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (string.IsNullOrEmpty(horario.HoraInicio))
            {
                Snackbar.Add("Por favor, introduce una Hora Inicio.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (string.IsNullOrEmpty(horario.HoraFin))
            {
                Snackbar.Add("Por favor, introduce una Hora Fin.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }

            horario.IdConcepto = Concepto.Id;
            horario.CodigoConcepto = Concepto.Codigo;

            var myContent = JsonConvert.SerializeObject(horario);

            var content = new StringContent(myContent, Encoding.UTF8, "application/json");
            var baseUrl = Configuration["UrlBackend"];
            var url = $"{baseUrl}/api/Horarios/CrearHorario";
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {

                await FetchHorarios().ConfigureAwait(false);

                Snackbar.Add("Horario creado con exito!", Severity.Success);

                horario.Descripcion = "";
                await horaInicio.ClearAsync();
                await horaFin.ClearAsync();


            }
            else
            {
                await FetchHorarios().ConfigureAwait(false);

                Snackbar.Add("Hubo un error al crear el horario!", Severity.Error);

            }



        }

        private async Task BorrarHorario(HorariosDTO datos)
        {
            var parameters = new DialogParameters<ModalBorrarHorario> { { x => x.Server, datos } };

            var dialog = await DialogService.ShowAsync<ModalBorrarHorario>("Borrar Horario", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                //In a real world scenario we would reload the data from the source here since we "removed" it in the dialog already.

                Console.WriteLine(result.Data.ToString());
                Console.WriteLine($"Event = Delete server, Data = {System.Text.Json.JsonSerializer.Serialize(result.Data)}");

                await FetchHorarios();
            }
        }

        private async Task ActualizarHorario(HorariosDTO item)
        {

            var myContent = JsonConvert.SerializeObject(item);
            var content = new StringContent(myContent, Encoding.UTF8, "application/json");
            var baseUrl = Configuration["UrlBackend"];
            var url = $"{baseUrl}/api/Horarios/ActualizarHorario";
            var response = await httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                var responseB = JsonConvert.DeserializeObject<ApiResponseDTO>(data);
                if (responseB.Success)
                {
                    Snackbar.Add("Horario actualizado con exito!", Severity.Success);
                    await FetchHorarios();
                    horario = new HorariosDTO();
                }
                else
                {
                    Snackbar.Add(responseB.Message, Severity.Error);
                }
            }
            else
            {
                await FetchHorarios();

                Snackbar.Add("Hubo un error al editar el horario!", Severity.Error);

            }
        }

        private async Task FetchHorarios()
        {
            try
            {
                Console.WriteLine("Traer usuarios");
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/Horarios/ListarHorarios";
                var response = await httpClient.GetAsync(url);
                Console.WriteLine("RespUsuarios");
                Console.WriteLine(response.ToString());
                
                if (response.IsSuccessStatusCode)
                {
                    var horarios = await response.Content.ReadFromJsonAsync<List<HorariosDTO>>();
                    if (horarios?.Count > 0)
                    {
                        ListaHorarios = horarios;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Snackbar.Add("Hubo un error al traer los usuarios", Severity.Error, config => { config.HideIcon = true; });
            }
        }

        private async Task BuscarConceptos()
        {
            var UrlGet = $"api/Horarios/BuscarConceptos";
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, BackdropClick = false };
            var parametros = new DialogParameters<BuscarLupa<ConceptoHorario>>
            {
                {x => x.Url, UrlGet},
                {x => x.OpcionesType, new CriterioConceptoHorario()},
                {x => x.Titulo, "Buscar Conceptos"}
            };
            var dialog = await DialogService.ShowAsync<BuscarLupa<ConceptoHorario>>("", parametros, options);
            var result = await dialog.Result;
            if (result != null && !result.Canceled && result.Data != null)
            {
                Console.WriteLine("Busqueda Concepto");

                CriterioConcepto = result.Data as ConceptoHorario;
                Concepto.Id = CriterioConcepto.Id;
                Concepto.Codigo = CriterioConcepto.Codigo;
                Concepto.Descripcion = CriterioConcepto.Descripcion;

            }
        }

        protected override async Task OnInitializedAsync()
        {

            await FetchHorarios();

        }

    }
}
