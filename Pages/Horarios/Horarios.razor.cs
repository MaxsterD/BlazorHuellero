using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ConsolaBlazor.Services.SessionStore;
using System.Net.Http.Json;
using System.Text.Json;
using ConsolaBlazor.Services.DTOs.Login;
using Microsoft.AspNetCore.Components.Web;
using ConsolaBlazor.CustomStyle;
using ConsolaBlazor.Services.DTOs.Horarios;
using static MudBlazor.CategoryTypes;
using ConsolaBlazor.Pages.Login;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ConsolaBlazor.Components.Modales;
using Newtonsoft.Json;
using System.Text;
using ConsolaBlazor.Services.DTOs.CreacionUsuario;
using ConsolaBlazor.Services.DTOs;

namespace ConsolaBlazor.Pages.Horarios
{
    public partial class Horarios
    {
        private HorariosDTO horario = new HorariosDTO();
        private List<HorariosDTO> ListaHorarios { get; set; } = new List<HorariosDTO>();

        [Inject] HttpClient httpClient { get; set; }
        [Inject] AuthenticationStateProvider autenticacionProvider { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IConfiguration Configuration { get; set; }

        private string titleBarStyle = $"height:7%;background-color:{AtowerTheme.Default.PaletteLight.Primary}; color:white;";
        private MudTimePicker horaInicio;
        private MudTimePicker horaFin;
        


        private async Task Guardar()
        {
            if (horario == null)
            {
                Snackbar.Add("Por favor, introduce los datos del horario.", Severity.Warning, config => { config.HideIcon = true; });
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

        protected override async Task OnInitializedAsync()
        {
            
            await FetchHorarios();

        }

    }
}
