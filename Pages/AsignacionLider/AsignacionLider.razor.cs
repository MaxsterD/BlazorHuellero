using ConsolaBlazor.Components.Modales;
using ConsolaBlazor.CustomStyle;
using ConsolaBlazor.Services.DTOs.Horarios;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ConsolaBlazor.Services.DTOs;
using Newtonsoft.Json;
using System.Text;
using ConsolaBlazor.Services.DTOs.AsignacionLider;
using ConsolaBlazor.Pages.Horarios;
using System.Net.Http.Json;
using ConsolaBlazor.Pages.Login;

namespace ConsolaBlazor.Pages.AsignacionLider
{
    public partial class AsignacionLider
    {
        private EmpleadosDTO Lider = new EmpleadosDTO();
        private EmpleadosDTO Empleado = new EmpleadosDTO();
        private AsignacionLiderDTO DatosAsignacion = new AsignacionLiderDTO();
        private List<EmpleadosLideresDTO> ListaEmpleadosUsuarios { get; set; } = new List<EmpleadosLideresDTO>();
        [Inject] HttpClient httpClient { get; set; }
        [Inject] AuthenticationStateProvider autenticacionProvider { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IConfiguration Configuration { get; set; }

        private string titleBarStyle = $"max-width:2000px;height:7%;background-color:{AtowerTheme.Default.PaletteLight.Primary}; color:white;";

        private EmpleadosBuscarDTO Criterio = new EmpleadosBuscarDTO();

        private async Task BuscarLideres()
        {
           
            var UrlGet = $"api/Usuarios/BuscarUsuario";
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, BackdropClick = false };
            var parametros = new DialogParameters<BuscarLupa<EmpleadosBuscarDTO>>
            {
                {x => x.Url, UrlGet},
                {x => x.OpcionesType, new CriterioEmpleadoDTO()},
                {x => x.Titulo, "Buscar Lideres"}
            };
            var dialog = await DialogService.ShowAsync <BuscarLupa<EmpleadosBuscarDTO>>("", parametros, options);
            var result = await dialog.Result;
            

            if (result != null && !result.Canceled && result.Data != null)
            {
                Criterio = result.Data as EmpleadosBuscarDTO;
                Lider.Id = Criterio.Id;
                Lider.Nombre = Criterio.Nombre;
                Lider.Identificacion = Criterio.Identificacion.ToString();
                
            }
        }

        private async Task BuscarEmpleados()
        {

            var UrlGet = $"api/Usuarios/BuscarUsuario";
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, BackdropClick = false };
            var parametros = new DialogParameters<BuscarLupa<EmpleadosBuscarDTO>>
            {
                {x => x.Url, UrlGet},
                {x => x.OpcionesType, new CriterioEmpleadoDTO()},
                {x => x.Titulo, "Buscar Empleados"}
            };
            var dialog = await DialogService.ShowAsync<BuscarLupa<EmpleadosBuscarDTO>>("", parametros, options);
            var result = await dialog.Result;


            if (result != null && !result.Canceled && result.Data != null)
            {
                Criterio = result.Data as EmpleadosBuscarDTO;
                Empleado.Id = Criterio.Id;
                Empleado.Nombre = Criterio.Nombre;
                Empleado.Identificacion = Criterio.Identificacion.ToString();
            }
        }

        private async Task BorrarUsuarioLider(EmpleadosLideresDTO datos)
        {
            var parameters = new DialogParameters<ModalBorrarEmpleadoLider> { { x => x.Server, datos } };

            var dialog = await DialogService.ShowAsync<ModalBorrarEmpleadoLider>("Borrar Registro", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                //In a real world scenario we would reload the data from the source here since we "removed" it in the dialog already.

                Console.WriteLine(result.Data.ToString());

                await FetchUsuariosLider();
            }
        }

        private async Task Guardar()
        {
            if (Lider == null)
            {
                Snackbar.Add("Por favor, introduce los datos del Lider.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }else if (Empleado == null)
            {
                Snackbar.Add("Por favor, introduce los datos del Empleado.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (string.IsNullOrEmpty(Lider.Nombre))
            {
                Snackbar.Add("Por favor, Selecciona un Lider.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (string.IsNullOrEmpty(Empleado.Nombre))
            {
                Snackbar.Add("Por favor, Selecciona un Empleado.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (Lider.Id == Empleado.Id)
            {
                Snackbar.Add("Un lider no puede ser su propio empleado", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }


            DatosAsignacion = new AsignacionLiderDTO(){ IdLider = Lider.Id, IdEmpleado = Empleado.Id };

            var myContent = JsonConvert.SerializeObject(DatosAsignacion);
            var content = new StringContent(myContent, Encoding.UTF8, "application/json");
            var baseUrl = Configuration["UrlBackend"];
            var url = $"{baseUrl}/api/AsignarLideres/GuardarEmpleadoLider";
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                var responseB = JsonConvert.DeserializeObject<ApiResponseDTO>(data);
                if (responseB.Success)
                {
                    Snackbar.Add("Empelado asignado al Lider con exito!", Severity.Success);
                    await FetchUsuariosLider().ConfigureAwait(false);
                    Lider = new EmpleadosDTO();
                    Empleado = new EmpleadosDTO();

                }
                else
                {
                    Snackbar.Add(responseB.Message, Severity.Error);
                }
            }
            else
            {
                await FetchUsuariosLider().ConfigureAwait(false);

                Snackbar.Add("Hubo un error al asignar el usuario al lider!", Severity.Error);

            }


        }

        private async Task Buscar()
        {
            EmpleadosLideresDTO datos = new EmpleadosLideresDTO();

            var UrlGet = $"api/Usuarios/BuscarUsuario";
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, BackdropClick = false };
            var parametros = new DialogParameters<BuscarLupa<EmpleadosBuscarDTO>>
            {
                {x => x.Url, UrlGet},
                {x => x.OpcionesType, new CriterioEmpleadoDTO()},
                {x => x.Titulo, "Buscar Lideres"}
            };
            var dialog = await DialogService.ShowAsync<BuscarLupa<EmpleadosBuscarDTO>>("", parametros, options);
            var result = await dialog.Result;


            if (result != null && !result.Canceled && result.Data != null)
            {
                
                Criterio = result.Data as EmpleadosBuscarDTO;
                datos.IdLider = Criterio.Id;
                await FetchUsuariosLider(datos);

            }
            
        }

        private async Task FetchUsuariosLider(EmpleadosLideresDTO? datos = null)
        {
            try
            {

                var myContent = JsonConvert.SerializeObject(datos);
                var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/AsignarLideres/ListarEmpleadosLider";
                var response = await httpClient.PostAsync(url,content);
                Console.WriteLine(response.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var registros = await response.Content.ReadFromJsonAsync<List<EmpleadosLideresDTO>>();
                    if (registros?.Count > 0)
                    {
                        ListaEmpleadosUsuarios = registros;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Snackbar.Add("Hubo un error al traer los usuarios", Severity.Error, config => { config.HideIcon = true; });
            }
        }

        protected override async Task OnInitializedAsync()
        {

            await FetchUsuariosLider();

        }

    }
}
