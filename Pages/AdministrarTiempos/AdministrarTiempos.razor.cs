using ConsolaBlazor.Components.Modales;
using ConsolaBlazor.CustomStyle;
using ConsolaBlazor.Services.DTOs.AsignarEmpleados;
using ConsolaBlazor.Services.DTOs.Horarios;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Extensions;
using static MudBlazor.CategoryTypes;
using System.Net.Http.Json;
using System.Text.Json;
using ConsolaBlazor.Services.DTOs.AdministrarTiempos;
using ConsolaBlazor.Services.DTOs;
using ConsolaBlazor.Services.DTOs.CreacionUsuario;
using Microsoft.Win32;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConsolaBlazor.Pages.AdministrarTiempos
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
                await FetchRegistros();
            }

            Console.WriteLine("Fuera del if");
            isLoading = false;
        }

        private async Task ActualizarRegistro(RegistrosTiemposDTO item)
        {
            Console.WriteLine(JsonSerializer.Serialize(item).ToString());

            //var myContent = JsonConvert.SerializeObject(item);
            //var content = new StringContent(JsonConvert.SerializeObject(myContent), Encoding.UTF8, "application/json");
            //var baseUrl = Configuration["UrlBackend"];
            //var url = $"{baseUrl}/api/Usuarios/ActualizarUsuario";
            //var response = await httpClient.PostAsync(url, content);
            //Console.WriteLine($"Event = CommittedItemChanges, Data = {System.Text.Json.JsonSerializer.Serialize(item)}");
            //Console.WriteLine(response.ToString());
        }

        private async Task Buscar()
        {

            try
            {
                isLoading = true;
                var fechaInicio = _dateRange?.Start.Value;
                var fechaFin = _dateRange?.End.Value;
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/ConectarHuellero/RecibirDatos";

                var requestBody = new
                {
                    IdUsuario = Empleado.Id?.ToString(),
                    FechaInicio = fechaInicio?.ToString("yyyy-MM-dd"),
                    FechaFin = fechaFin?.ToString("yyyy-MM-dd")
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
                var fechaInicio =_dateRange?.Start.Value;
                var fechaFin = _dateRange?.End.Value;
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/ConectarHuellero/RecibirDatos";

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
                var url = $"{baseUrl}/api/ConectarHuellero/AlimentarBase";


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

            if(x.EstadoSalida == null || x.EstadoEntrada == "Tarde")
                return "background-color:#ff8b8b8f";

            return "";
        };

    }
}
