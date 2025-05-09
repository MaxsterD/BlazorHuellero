﻿using BlazorAppHuellero.Components.Modales;
using BlazorAppHuellero.CustomStyle;
using BlazorAppHuellero.Services.DTOs;
using BlazorAppHuellero.Services.DTOs.CreacionUsuario;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace BlazorAppHuellero.Pages.CreacionUsuario
{
    public partial class CreacionUsuario
    {
        private UsuarioDTO Usuario = new UsuarioDTO();
        private List<TiposIdentificacionDTO> Tipos_Identificacion { get; set; } = new List<TiposIdentificacionDTO>();
        private List<UsuarioDTO> Usuarios { get; set; } = new List<UsuarioDTO>();

        [Inject] HttpClient httpClient { get; set; }
        [Inject] AuthenticationStateProvider autenticacionProvider { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IConfiguration Configuration { get; set; }

        private string titleBarStyle = $"height:7%;background-color:{AtowerTheme.Default.PaletteLight.Primary}; color:white;";

        private UsuarioDTO Criterio = new UsuarioDTO();

        private async Task Guardar()
        {
            if (Usuario == null)
            {
                Snackbar.Add("Por favor, introduce los datos del usuario.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (string.IsNullOrEmpty(Usuario.Nombre))
            {
                Snackbar.Add("Por favor, Ingrese un nombre.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (string.IsNullOrEmpty(Usuario.Tipo_Identificacion))
            {
                Snackbar.Add("Por favor, Seleccione un Tipo de Identificacion.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }
            else if (!Usuario.Identificacion.HasValue || Usuario.Identificacion == 0)
            {
                Snackbar.Add("Por favor, Ingrese un Numero de Identificacion.", Severity.Warning, config => { config.HideIcon = true; });
                return;
            }

            var myContent = JsonConvert.SerializeObject(Usuario);
            var content = new StringContent(myContent, Encoding.UTF8, "application/json");
            var baseUrl = Configuration["UrlBackend"];
            var url = $"{baseUrl}/api/Usuarios/CrearUsuario";
            var response = await httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                var responseB = JsonConvert.DeserializeObject<ApiResponseDTO>(data);
                if (responseB.Success)
                {
                    Snackbar.Add("Usuario creado con exito!", Severity.Success);
                    await FetchUsuarios().ConfigureAwait(false);
                    Usuario.Nombre = "";
                    Usuario.Tipo_Identificacion = "";
                    Usuario.Identificacion = null;
                    Usuario.Id = 0;
                }
                else
                {

                    Snackbar.Add(responseB.Message, Severity.Error);


                }






            }
            else
            {
                await FetchUsuarios().ConfigureAwait(false);

                Snackbar.Add("Hubo un error al crear el usuario!", Severity.Error);

            }



        }

        private async Task BorrarUsuario(UsuarioDTO datos)
        {
            var parameters = new DialogParameters<ModalBorrarUsuario> { { x => x.Server, datos } };

            var dialog = await DialogService.ShowAsync<ModalBorrarUsuario>("Borrar Usuario", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                //In a real world scenario we would reload the data from the source here since we "removed" it in the dialog already.

                Console.WriteLine(result.Data.ToString());
                Console.WriteLine($"Event = Delete server, Data = {System.Text.Json.JsonSerializer.Serialize(result.Data)}");

                await FetchUsuarios();
            }
        }

        private async Task ActualizarUsuario(UsuarioDTO item)
        {
            var myContent = JsonConvert.SerializeObject(item);
            var content = new StringContent(myContent, Encoding.UTF8, "application/json");
            var baseUrl = Configuration["UrlBackend"];
            var url = $"{baseUrl}/api/Usuarios/ActualizarUsuario";
            var response = await httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                var responseB = JsonConvert.DeserializeObject<ApiResponseDTO>(data);
                if (responseB.Success)
                {
                    Snackbar.Add(responseB.Message, Severity.Success, config => { config.HideIcon = true; });
                    await FetchUsuarios().ConfigureAwait(false);
                }
                else
                {
                    Snackbar.Add(responseB.Message, Severity.Error);
                }
            }
            else
            {
                await FetchUsuarios().ConfigureAwait(false);

                Snackbar.Add("Hubo un error al actualizar el usuario!", Severity.Error);

            }


        }

        private async Task FetchUsuarios()
        {
            try
            {
                Console.WriteLine("Traer usuarios");
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/Usuarios/ListarUsuarios";
                var response = await httpClient.GetAsync(url);
                Console.WriteLine("RespUsuarios");
                Console.WriteLine(response.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDTO>>();
                    if (usuarios?.Count > 0)
                    {
                        Usuarios = usuarios;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Snackbar.Add("Hubo un error al traer los usuarios", Severity.Error, config => { config.HideIcon = true; });
            }
        }

        private async Task FetchTipoIdentificacion()
        {
            try
            {
                var baseUrl = Configuration["UrlBackend"];
                var url = $"{baseUrl}/api/AsignarLideres/ListarIdentificacion";
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var tiposIdentificacion = await response.Content.ReadFromJsonAsync<List<TiposIdentificacionDTO>>();
                    if (tiposIdentificacion?.Count > 0)
                    {
                        Tipos_Identificacion = tiposIdentificacion;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Snackbar.Add("Hubo un error al traer los tipos de identificacion", Severity.Error, config => { config.HideIcon = true; });
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await FetchTipoIdentificacion();
            await FetchUsuarios();

        }
    }
}
