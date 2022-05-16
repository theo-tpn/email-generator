using ElectronNET.API;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Logging;
using ElectronNET.API.Entities;

namespace Noctus.GenWave.Desktop.App.Managers
{
    public class AppManager
    {
        private BrowserWindow _browserWindow;
        private DiscordRpcClient _discordClient;
        
        public async Task Initialize(bool isDevelopment = false)
        {
            if (isDevelopment)
            {
                await Electron.WindowManager.CreateBrowserViewAsync();
            }

            _browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Frame = false,
                MinWidth = 1280,
                MinHeight = 720,
                Title = "Genwave",
                Icon = "build/icon.ico"
            });

            _discordClient = new DiscordRpcClient("822792424543748097")
            {
                Logger = new ConsoleLogger()
            };

            _discordClient.OnReady += (_, e) =>
            {
                _discordClient.Logger.Info("User {0}", e.User.Username);
                _discordClient.SetPresence(new RichPresence
                {
                    State = AppConstants.AppVersion,
                    Details = "flexin'",
                    Assets = new Assets
                    {
                        LargeImageKey = "logo",
                        LargeImageText = "@genwave_rcm"
                    },
                    Timestamps = Timestamps.Now
                });
            };
            
            _discordClient.OnError += (_, e) =>
            {
                _discordClient.Logger.Info("Error! {0}", e.Message);
            };

            _discordClient.Initialize();
        }

        public void MinimizeApp()
        {
            _browserWindow.Minimize();
        }

        public async Task Restore()
        {
            if (!await _browserWindow.IsMaximizedAsync())
            {
                _browserWindow.Maximize();
            }
            else
            {
                _browserWindow.Restore();
            }
        }

        public void CloseApp()
        {
            _browserWindow.Close();
            _discordClient.Dispose();
        }
    }
}
