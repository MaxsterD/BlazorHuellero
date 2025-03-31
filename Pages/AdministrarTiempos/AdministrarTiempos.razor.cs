using BlazorAppHuellero.Components.Modales;
using BlazorAppHuellero.CustomStyle;
using BlazorAppHuellero.Services.DTOs;
using BlazorAppHuellero.Services.DTOs.AdministrarTiempos;
using BlazorAppHuellero.Services.DTOs.AsignarEmpleados;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;
using System.Text.Json;

namespace BlazorAppHuellero.Pages.AdministrarTiempos
{
    public partial class AdministrarTiempos
    {
        private DateRange _dateRange = null;
        private EmpleadosDTO Empleado = new EmpleadosDTO();
        private List<RegistrosTiemposDTO> Registros { get; set; } = new List<RegistrosTiemposDTO>();
        private RegistrosTiemposDTO Registro { get; set; } = new RegistrosTiemposDTO();
        public bool isLoading = false;  // Variable para controlar el estado de carga

        [Inject] HttpClient httpClient { get; set; }
        [Inject] AuthenticationStateProvider autenticacionProvider { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IConfiguration Configuration { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }


        private string titleBarStyle = $"height:7%;background-color:{AtowerTheme.Default.PaletteLight.Primary}; color:white;";

        private EmpleadosBuscarDTO Criterio = new EmpleadosBuscarDTO();

        private async Task BuscarEmpleados()
        {

            var UrlGet = $"api/Usuarios/ListarUsuarios";
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, BackdropClick = false };
            var parametros = new DialogParameters<BuscarLupa<EmpleadosBuscarDTO>>
            {
                {x => x.Url, UrlGet},
                {x => x.OpcionesType, new CriterioEmpleadoDTO()},
                {x => x.Titulo, "Buscar Usuario"}
            };
            var dialog = await DialogService.ShowAsync<BuscarLupa<EmpleadosBuscarDTO>>("", parametros, options);
            var result = await dialog.Result;


            if (result != null && !result.Canceled && result.Data != null)
            {
                Criterio = result.Data as EmpleadosBuscarDTO;
                Empleado.Id = Criterio.Id;
                Empleado.Nombre = Criterio.Nombre;
                Empleado.TipoIdentificacion = Criterio.Tipo_Identificacion;
                Empleado.Identificacion = Criterio.Identificacion.ToString();

            }
        }

        private async Task BorrarRegistro(RegistrosTiemposDTO datos)
        {
            isLoading = true;
            var parameters = new DialogParameters<ConfirmActionModalRegistros> { { x => x.Server, datos }, { x => x.isLoading, isLoading } };
            var dialog = await DialogService.ShowAsync<ConfirmActionModalRegistros>("Borrar Registro", parameters);
            var result = await dialog.Result;
            Console.WriteLine("Result");

            if (!result.Canceled)
            {
                Console.WriteLine("Se borro");
                await Task.Delay(1500);
                isLoading = true;
                await Buscar(null);
            }

            Console.WriteLine("Fuera del if");
            isLoading = false;
        }

        private async Task ActualizarRegistro(RegistrosTiemposDTO item)
        {

            if ((item.HoraEntrada == null || item.HoraEntrada == "") && (item.HoraSalida == null || item.HoraSalida == ""))
            {
                Snackbar.Add("Ambos registros no pueden estar vacios", Severity.Error);
                await Buscar(null);

            }
            else
            {
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(item), Encoding.UTF8, "application/json");
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/Huellero/ActualizarRegistro";
                var response = await httpClient.PostAsync(url, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content.ReadAsStringAsync().Result;
                    var responseB = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseDTO>(data);
                    if (responseB.Success)
                    {
                        Snackbar.Add(responseB.Message, Severity.Success, config => { config.HideIcon = true; });
                        if (DateTime.TryParse(item.Fecha, out DateTime fecha))
                        {
                            await Buscar(fecha);
                        }
                    }
                    else
                    {
                        Snackbar.Add(responseB.Message, Severity.Error);
                    }
                }
                else
                {
                    if (DateTime.TryParse(item.Fecha, out DateTime fecha))
                    {
                        await Buscar(fecha);
                    }

                    Snackbar.Add("Hubo un error al actualizar el registro!", Severity.Error);

                }
                Console.WriteLine($"Event = CommittedItemChanges, Data = {System.Text.Json.JsonSerializer.Serialize(item)}");
                Console.WriteLine(response.ToString());
            }


        }

        private async Task Buscar(DateTime? fecha)
        {

            try
            {
                var fechaInicio = "";
                var fechaFin = "";
                if (fecha.HasValue) // Verifica si 'fecha' tiene un valor
                {
                    fechaInicio = fecha.Value.ToString("yyyy-MM-dd"); // Usa .Value para acceder al DateTime subyacente
                    fechaFin = fecha.Value.ToString("yyyy-MM-dd");
                }
                else
                {
                    fechaInicio = _dateRange?.Start.Value.ToString("yyyy-MM-dd");
                    fechaFin = _dateRange?.End.Value.ToString("yyyy-MM-dd");
                }
                isLoading = true;

                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/Huellero/RecibirDatos";

                var requestBody = new
                {
                    IdUsuario = Empleado.Id?.ToString(),
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(responseString);
                if (apiResponse?.Data == null)
                {
                    Snackbar.Add(apiResponse?.Message, Severity.Error, config => { config.HideIcon = true; });
                    return;
                }

                Registros = JsonSerializer.Deserialize<List<RegistrosTiemposDTO>>(apiResponse.Data.ToString());
                isLoading = false;

            }
            catch (Exception ex)
            {
                Snackbar.Add("Hubo un error al traer los usuarios", Severity.Error, config => { config.HideIcon = true; });
                isLoading = false;
            }


        }

        private async Task FetchRegistros()
        {
            try
            {
                isLoading = true;
                var fechaInicio = _dateRange?.Start.Value;
                var fechaFin = _dateRange?.End.Value;
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/Huellero/RecibirDatos";

                var requestBody = new
                {
                    IdUsuario = Empleado.Id?.ToString(),
                    FechaInicio = DateTime.Now.ToString("yyyy-MM-dd"),
                    FechaFin = DateTime.Now.ToString("yyyy-MM-dd")
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(responseString);
                if (apiResponse?.Data == null)
                {
                    isLoading = false;
                    Snackbar.Add(apiResponse?.Message, Severity.Error, config => { config.HideIcon = true; });
                    return;
                }

                Registros = JsonSerializer.Deserialize<List<RegistrosTiemposDTO>>(apiResponse.Data.ToString());

                _dateRange = null;
                Empleado = new EmpleadosDTO();

                isLoading = false;

            }
            catch (Exception ex)
            {
                Snackbar.Add("Hubo un error al traer los usuarios", Severity.Error, config => { config.HideIcon = true; });
                isLoading = false;
            }
        }

        protected override async Task OnInitializedAsync()
        {

            await FetchRegistros();

        }

        private async Task AlimentarBase()
        {
            try
            {
                isLoading = true;
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/Huellero/AlimentarBase";


                var response = await httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(responseString);
                if (apiResponse?.Data == null)
                {
                    Snackbar.Add(apiResponse?.Message, Severity.Error, config => { config.HideIcon = true; });
                    isLoading = false;
                    return;
                }

                if (apiResponse.Success == true)
                {
                    isLoading = false;
                    Snackbar.Add(apiResponse?.Message, Severity.Success, config => { config.HideIcon = true; });
                    await FetchRegistros();
                    return;

                }
                else
                {
                    Snackbar.Add(apiResponse?.Message, Severity.Error, config => { config.HideIcon = true; });
                    isLoading = false;
                    return;
                }


            }
            catch (Exception ex)
            {
                Snackbar.Add("Hubo un error al traer los usuarios", Severity.Error, config => { config.HideIcon = true; });
                isLoading = false;
            }
        }

        private Func<RegistrosTiemposDTO, int, string> _rowStyleFunc => (x, i) =>
        {
            if (x.EstadoEntrada == null || x.EstadoEntrada == "Tarde")
                return "background-color:#ff8b8b8f";

            if (x.EstadoSalida == null || x.EstadoEntrada == "Tarde")
                return "background-color:#ff8b8b8f";

            return "";
        };

        private async Task ExportToPdf()
        {
            if (Registros is null || !Registros.Any())
            {
                // Verifica si la lista es nula o si está vacía
                Snackbar.Add("No hay registros que mostrar!", Severity.Error);
            }
            else
            {
                // Preparar los datos para el PDF (pueden provenir de una base de datos o un MudDataGrid)
                var data = Registros;

                // Llamar a la función de JavaScript para generar el PDF
                await JSRuntime.InvokeVoidAsync("generatePdf", data);
            }
        }
    }
}
