using BlazorAppHuellero.Services.DTOs.Login;

namespace BlazorAppHuellero.Services.SessionStore.StoreSession
{
    public class ClienteSession
    {
        public SesionDTO UserSession { get; private set; }

        public void SetuserSession(SesionDTO session)
        {
            UserSession = session;
        }
    }
}
