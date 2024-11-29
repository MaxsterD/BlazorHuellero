using ConsolaBlazor.Components.Modales;
using ConsolaBlazor.CustomStyle;
using ConsolaBlazor.Services.DTOs.CreacionUsuario;
using ConsolaBlazor.Services.DTOs.Horarios;
using ConsolaBlazor.Services.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using ConsolaBlazor.Services.DTOs.Parametros;

namespace ConsolaBlazor.Pages.Parametros
{
    public partial class Parametros
    {

        [Inject] HttpClient httpClient { get; set; }
        [Inject] AuthenticationStateProvider autenticacionProvider { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IConfiguration Configuration { get; set; }

        private ParametrosDTO Parametro = new ParametrosDTO();
        private List<ParametrosDTO> ListaParametros = new List<ParametrosDTO>();
        private string titleBarStyle = $"height:7%;background-color:{AtowerTheme.Default.PaletteLight.Primary}; color:white;";

        private async Task ActualizarParametro(ParametrosDTO item)
        {
            try
            {
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/Parametros/ActualizarParametro";
                var requestBody = item;

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(responseString);

                if (response.IsSuccessStatusCode)
                {
                    
                    Snackbar.Add(apiResponse.Message, Severity.Success, config => { config.HideIcon = true; });
                    await FetchParametros();
                }
                else
                {
                    Snackbar.Add(apiResponse.Message, Severity.Error, config => { config.HideIcon = true; });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Snackbar.Add("Hubo un error al traer los usuarios", Severity.Error, config => { config.HideIcon = true; });
            }
            
           
        }

        private async Task FetchParametros()
        {
            try
            {
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/Parametros/ListarParametros";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var parametros = await response.Content.ReadFromJsonAsync<List<ParametrosDTO>>();
                    if (parametros?.Count > 0)
                    {
                        ListaParametros = parametros;

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
            await FetchParametros();

        }
    }
}
