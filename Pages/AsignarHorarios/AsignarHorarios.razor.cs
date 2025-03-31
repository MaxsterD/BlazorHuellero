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

namespace BlazorAppHuellero.Pages.AsignarHorarios
{
    public partial class AsignarHorarios
    {
        private UsuarioDTO Usuario = new UsuarioDTO();
        private HorariosDTO Horario = new HorariosDTO();
        private List<HorariosUsuariosDTO> HorariosUsuarios { get; set; } = new List<HorariosUsuariosDTO>();


        [Inject] HttpClient httpClient { get; set; }
        [Inject] AuthenticationStateProvider autenticacionProvider { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IConfiguration Configuration { get; set; }

        private string titleBarStyle = $"height:7%;background-color:{AtowerTheme.Default.PaletteLight.Primary}; color:white;";

        private UsuarioDTO CriterioUsuario = new UsuarioDTO();
        private HorariosDTO CriterioHorario = new HorariosDTO();

        private async Task Guardar()
        {

            if (Usuario == null || string.IsNullOrEmpty(Usuario.Nombre) || string.IsNullOrEmpty(Usuario.Tipo_Identificacion) || !Usuario.Identificacion.HasValue || Usuario.Identificacion == 0)
            {
                Snackbar.Add("Por favor, seleccione un usuario.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (Horario == null || string.IsNullOrEmpty(Horario.Descripcion) || string.IsNullOrEmpty(Horario.HoraInicio) || string.IsNullOrEmpty(Horario.HoraFin))
            {
                Snackbar.Add("Por favor, seleccione un horario.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }

            HorariosUsuariosDTO HorarioAsignar = new HorariosUsuariosDTO();
            HorarioAsignar.IdUsuario = CriterioUsuario.Id;
            HorarioAsignar.IdHorario = CriterioHorario.Id;

            var myContent = JsonConvert.SerializeObject(HorarioAsignar);
            var content = new StringContent(myContent, Encoding.UTF8, "application/json");
            var baseUrl = Configuration["UrlBackend"];
            var url = $"{baseUrl}/api/AsignarHorario/AsignarHorario";
            var response = await httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                var responseB = JsonConvert.DeserializeObject<ApiResponseDTO>(data);
                if (responseB.Success)
                {
                    Snackbar.Add("Usuario creado con exito!", Severity.Success);
                    await FetchRegistros().ConfigureAwait(false);
                    Usuario.Nombre = "";
                    Usuario.Tipo_Identificacion = "";
                    Usuario.Identificacion = null;
                    Usuario.Id = 0;

                    Horario.Descripcion = "";
                    Horario.HoraInicio = "";
                    Horario.HoraFin = "";
                    Horario.Id = 0;
                }
                else
                {
                    Snackbar.Add(responseB.Message, Severity.Error);
                }
            }
            else
            {
                await FetchRegistros().ConfigureAwait(false);

                Snackbar.Add("Hubo un error al crear el usuario!", Severity.Error);

            }
        }

        private async Task BuscarUsuarios()
        {
            var UrlGet = $"api/Usuarios/BuscarUsuario";
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, BackdropClick = false };
            var parametros = new DialogParameters<BuscarLupa<UsuarioDTO>>
            {
                {x => x.Url, UrlGet},
                {x => x.OpcionesType, new CriterioUsuarioDTO()},
                {x => x.Titulo, "Buscar Usuarios"}
            };
            var dialog = await DialogService.ShowAsync<BuscarLupa<UsuarioDTO>>("", parametros, options);
            var result = await dialog.Result;
            if (result != null && !result.Canceled && result.Data != null)
            {
                Console.WriteLine("Busqueda Usuario");

                CriterioUsuario = result.Data as UsuarioDTO;
                Usuario.Id = CriterioUsuario.Id;
                Usuario.Nombre = CriterioUsuario.Nombre;
                Usuario.Identificacion = CriterioUsuario.Identificacion;
                Usuario.Tipo_Identificacion = CriterioUsuario.Tipo_Identificacion;

            }
        }

        private async Task BuscarHorarios()
        {
            var UrlGet = $"api/Horarios/BuscarHorarios";
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, BackdropClick = false };
            var parametros = new DialogParameters<BuscarLupa<HorariosDTO>>
            {
                {x => x.Url, UrlGet},
                {x => x.OpcionesType, new CriterioHorariosDTO()},
                {x => x.Titulo, "Buscar Horarios"}
            };
            var dialog = await DialogService.ShowAsync<BuscarLupa<HorariosDTO>>("", parametros, options);
            var result = await dialog.Result;
            if (result != null && !result.Canceled && result.Data != null)
            {
                CriterioHorario = result.Data as HorariosDTO;
                Usuario.Id = CriterioHorario.Id;
                Horario.Descripcion = CriterioHorario.Descripcion;
                Horario.HoraInicio = CriterioHorario.HoraInicio;
                Horario.HoraFin = CriterioHorario.HoraFin;

            }
        }

        private async Task BorrarRegistro(HorariosUsuariosDTO datos)
        {
            var parameters = new DialogParameters<ModalBorrarRegistro> { { x => x.Server, datos } };

            var dialog = await DialogService.ShowAsync<ModalBorrarRegistro>("Borrar Registro", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                //In a real world scenario we would reload the data from the source here since we "removed" it in the dialog already.

                Console.WriteLine(result.Data.ToString());
                Console.WriteLine($"Event = Delete server, Data = {System.Text.Json.JsonSerializer.Serialize(result.Data)}");

                await FetchRegistros();
            }
        }

        private async Task ActualizarRegistro(HorariosUsuariosDTO item)
        {
            var myContent = JsonConvert.SerializeObject(item);
            var content = new StringContent(myContent, Encoding.UTF8, "application/json");
            var baseUrl = Configuration["UrlBackend"];
            var url = $"{baseUrl}/api/Usuarios/ActualizarUsuario";
            var response = await httpClient.PostAsync(url, content);
            Console.WriteLine($"Event = CommittedItemChanges, Data = {System.Text.Json.JsonSerializer.Serialize(item)}");
            Console.WriteLine(response.ToString());
        }

        private async Task FetchRegistros()
        {
            try
            {
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/AsignarHorario/ListarHorariosUsuarios";
                var response = await httpClient.GetAsync(url);
                Console.WriteLine("RespUsuarios");
                Console.WriteLine(response.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var registros = await response.Content.ReadFromJsonAsync<List<HorariosUsuariosDTO>>();
                    if (registros?.Count > 0)
                    {
                        HorariosUsuarios = registros;

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
            await FetchRegistros();

        }
    }
}
