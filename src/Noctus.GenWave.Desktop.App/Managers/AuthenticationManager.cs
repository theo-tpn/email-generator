#nullable enable
using Microsoft.Extensions.Options;
using Noctus.Application.Helpers;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using Stl.Async;
using Stl.Fusion;
using System.Reflection;
using System.Threading.Tasks;

namespace Noctus.GenWave.Desktop.App.Managers
{
    [ComputeService]
    public class AuthenticationManager
    {
        private readonly INoctusService _noctusService;

        private readonly UserState _userState = new();

        public AuthenticationManager(INoctusService noctusService)
        {
            _noctusService = noctusService;
        }

        [ComputeMethod(AutoInvalidateTime = 120, KeepAliveTime = 120)]
        public virtual async Task<UserState> GetUserState()
        {
            if (_userState.IsLogged)
                await RenewLogin();
            return _userState;
        }

        public async Task<bool> Login(string license, KeyEvent keyEvent, string? discordId = null)
        {
            _userState.IsLogged = await _noctusService.Login(license, MachineInfoHelper.BuildLicenseIdentifiersInfo().MotherBoardSerialNumber, keyEvent, discordId);
            _userState.LicenseKey = license;

            return _userState.IsLogged;
        }

        public async Task RenewLogin()
        {
            _userState.IsLogged = await Login(_userState.LicenseKey, KeyEvent.RenewLogin);
        }

        public async Task Logout()
        {
            if (!_userState.IsLogged)
                return;
            _userState.IsLogged = false;
            await _noctusService.Logout(_userState.LicenseKey,
                MachineInfoHelper.BuildLicenseIdentifiersInfo().MotherBoardSerialNumber, _userState.DiscordId);
        }
    }
}
