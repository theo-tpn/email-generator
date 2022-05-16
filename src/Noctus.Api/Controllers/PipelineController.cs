using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.UnitOfWork;
using Noctus.Domain.Models.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PipelineController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly ILicenseKeyRepository _licenseKeyRepository;
        private readonly IPipelineRunRepository _pipelineRunRepository;

        private readonly IGenBucketService _genBucketService;

        public PipelineController(IUnitOfWork uow, ILicenseKeyRepository licenseKeyRepository, IPipelineRunRepository pipelineRunRepository, IGenBucketService genBucketService)
        {
            _uow = uow;
            _licenseKeyRepository = licenseKeyRepository;
            _pipelineRunRepository = pipelineRunRepository;

            _genBucketService = genBucketService;
        }

        [HttpPost("")]
        public async Task<ActionResult> CreatePipeline([FromBody] PipelineRunDto pipelineRun)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                if (!_licenseKeyRepository.TryGetByKey(pipelineRun.LicenseKey, out var key))
                    return BadRequest("Key not found");
                var pipeline = _pipelineRunRepository.Create(new PipelineRun()
                {
                    AccountCountryCode = pipelineRun.AccountCountryCode,
                    HasForwarding = pipelineRun.HasForwarding,
                    JobsNumber = pipelineRun.Jobs,
                    JobsParallelism = pipelineRun.Parallelism,
                    PvaCountryCode = pipelineRun.PvaCountryCode
                });
                key.PipelineRuns.Add(pipeline);
                _licenseKeyRepository.Update(key);
                await _uow.Commit();
                return Ok(pipeline.Id);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("{id}/event/{pipelineEvent}")]
        public async Task<ActionResult> AddPipelineEvent(int id, int pipelineEvent)
        {
            try
            {
                var pipeline = _pipelineRunRepository.Get(id);
                if (pipeline == null) return BadRequest("no pipelineRun found");
                var isEventDefined = Enum.IsDefined(typeof(PipelineEventType), pipelineEvent);
                if (!isEventDefined) return BadRequest($"{pipelineEvent} is not an event");
                pipeline.Events.Add(new PipelineEvent { EventType = (PipelineEventType)pipelineEvent });
                _pipelineRunRepository.Update(pipeline);
                await _uow.Commit();
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("{pipelineId}/job/finish")]
        public async Task<ActionResult> FinishJob(int pipelineId)
        {
            try
            {
                var pipeline = _pipelineRunRepository.Get(pipelineId);
                if (pipeline == null) return BadRequest("no pipeline found");
                var key = _licenseKeyRepository.GetAll().SingleOrDefault(x => x.PipelineRuns.Contains(pipeline));
                if (key == null) return BadRequest("no assiocated key found");
                var result = _genBucketService.DecreaseBucket(key.AccountsGenBuckets.First());
                await _uow.Commit();
                return result ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
