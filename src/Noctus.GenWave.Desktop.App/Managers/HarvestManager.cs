using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Infrastructure;
using Stl.Async;
using Stl.Fusion;

namespace Noctus.GenWave.Desktop.App.Managers
{
    [ComputeService]
    public class HarvestManager
    {
        private readonly IHarvestedCookiesRepository _repository;

        public HarvestManager(IHarvestedCookiesRepository repository)
        {
            _repository = repository;
        }

        private int _remainingTasks;

        [ComputeMethod(AutoInvalidateTime = 1)]
        public virtual async Task<int> HarvestingTasksCounter()
        {
            return await Task.FromResult(_remainingTasks);
        }

        public async Task Harvest(int count)
        {
            _remainingTasks = count;
            
            var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Width = 300,
                Height = 300,
                Title = "Harvester",
                Icon = "build/icon.ico",
                Frame = false,
                Resizable = false,
                Closable = false
            }, $"http://localhost:{BridgeSettings.WebPort}/harvester");

            var browserView = await Electron.WindowManager.CreateBrowserViewAsync(new BrowserViewConstructorOptions());
            window.SetBrowserView(browserView);

            while (_remainingTasks > 0)
            {
                browserView.WebContents.Session.SetUserAgent(
                    UserAgents.List[RandomGenerator.Random.Next(0, UserAgents.List.Count)]);
         
                await browserView.WebContents.LoadURLAsync("https://www.google.com");
                await browserView.WebContents.LoadURLAsync("https://www.nytimes.com/");
                await browserView.WebContents.LoadURLAsync("https://www.lemonde.fr/");
                await browserView.WebContents.LoadURLAsync("https://www.outlook.com");

                var cookies = await browserView.WebContents.Session.Cookies.GetAsync(new CookieFilter());

                _repository.Insert(cookies.Select(c => new System.Net.Cookie(c.Name, c.Value, c.Path, c.Domain)
                    {HttpOnly = c.HttpOnly, Secure = c.Secure}).ToList());

                await browserView.WebContents.Session.ClearStorageDataAsync();

                _remainingTasks--;
            }

            window.Destroy();

            using (Computed.Invalidate())
            {
                Availability().Ignore();
            }
        }

        [ComputeMethod(AutoInvalidateTime = 60, KeepAliveTime = 61)]
        public virtual async Task<int> Availability()
        {
            return await Task.FromResult(_repository.Count());
        }
    }
}
