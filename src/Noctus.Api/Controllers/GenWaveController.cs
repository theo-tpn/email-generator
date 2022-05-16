using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.UnitOfWork;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using Noctus.Domain.Models.Dto;

namespace Noctus.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenwaveController : ControllerBase
    {
        private readonly ILogger<GenwaveController> _logger;
        private readonly IUnitOfWork _uow;
        private readonly ILicenseKeyRepository _licenseKeyRepository;

        private readonly ILicenseKeyService _licenseKeyService;
        private readonly IGenBucketService _genBucketService;
        private readonly IMetalabsService _metalabsService;


        public GenwaveController(
            ILogger<GenwaveController> logger, 
            IUnitOfWork uow,
            ILicenseKeyRepository licenseKeyRepositiory,
            IMetalabsService metalabsService,
            ILicenseKeyService licenseKeyService,
            IGenBucketService genBucketService)
        {
            _logger = logger;
            _uow = uow;
            _licenseKeyRepository = licenseKeyRepositiory;

            _licenseKeyService = licenseKeyService;
            _genBucketService = genBucketService;
            _metalabsService = metalabsService;
        }

        [HttpGet]
        public string Index() => "Hi curious :)";

        [HttpPost("license/login")]
        public async Task<ActionResult> Login([FromBody] LicenseEventDto info)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var metalabsKey = await _metalabsService.GetMetalabsLicense(info.LicenseKey);
                if (metalabsKey == null || !string.Equals(metalabsKey.Status, "active"))
                {
                    return NotFound("Key is not valid.");
                }

                var identifierInfo = new IdentifiersInfo()
                {
                    MotherBoardSerialNumber = info.MbSerialInfo,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserDiscordId = metalabsKey.User.Discord.Id
                };
                var lke = new LicenseKeyEvent
                {
                    IdentifiersInfo = identifierInfo,
                    Event = info.Event,
                };

                if (!_licenseKeyRepository.TryGetByKey(info.LicenseKey, out var key))
                {
                    key = _licenseKeyService.RegisterKey(info.LicenseKey, identifierInfo, metalabsKey.Plan.Id);
                    _genBucketService.FillBucket(key.AccountsGenBuckets.First());
                }
                var isKeyValid = _licenseKeyService.IsLicenseKeyValid(key);
                if (!isKeyValid)
                {
                    lke.Event = KeyEvent.LoginFailed;
                }

                _licenseKeyService.AddKeyEvent(key, lke);
                await _uow.Commit();
                return lke.Event == KeyEvent.LoginFailed ? BadRequest() : Ok();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        [HttpPost("license/logout")]
        public async Task<ActionResult> Logout([FromBody] LicenseEventDto info)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                if (!_licenseKeyRepository.TryGetByKey(info.LicenseKey, out var key))
                    return BadRequest();

                _licenseKeyService.AddKeyEvent(key, new LicenseKeyEvent
                {
                    Event = KeyEvent.Logout,
                    IdentifiersInfo = new IdentifiersInfo()
                    {
                        IpAddress = ControllerContext.HttpContext.Connection.RemoteIpAddress?.ToString(),
                        MotherBoardSerialNumber = info.MbSerialInfo,
                        UserDiscordId = info.UserDiscordId ?? string.Empty
                    }
                });
                await _uow.Commit();
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("license/flags")]
        public async Task<ActionResult> PostLicenseKeyFlag([FromBody] LicenseFlagDto info)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                if (_licenseKeyRepository.TryGetByKey(info.LicenseKey, out var key))
                    return BadRequest();
                key.KeyFlags.Add(new LicenseKeyFlag()
                {
                    Status = info.Flag,
                    Reason = info.Reason,
                    IdentifiersInfo = new IdentifiersInfo()
                    {
                        IpAddress = ControllerContext.HttpContext.Connection.RemoteIpAddress?.ToString(),
                        MotherBoardSerialNumber = info.MbSerialInfo
                    }
                });
                _licenseKeyRepository.Update(key);
                await _uow.Commit();
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("license/{licenseKey}/bucket")]
        public async Task<ActionResult<AccountGenBucket>> GetLicenseBucket(string licenseKey)
        {
            try
            {
                if (!_licenseKeyRepository.TryGetByKey(licenseKey, out var key))
                    return BadRequest();
                var genBucketConfig = _genBucketService.GetBucketConfig(key.AccountsGenBuckets.First());
                return Ok(new AccountGenBucket()
                {
                    CurrentStock = key.AccountsGenBuckets.First().CurrentStock,
                    MaximumStock = genBucketConfig.Quantity,
                    NextRefillAt = DateTime.Now.AddDays(1)
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
