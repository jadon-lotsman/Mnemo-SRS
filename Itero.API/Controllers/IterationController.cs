using System.Security.Claims;
using Itero.API.Common;
using Itero.API.Data.Entities;
using Itero.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Itero.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class IterationController : ControllerBase
    {
        public IterationController(IterationService service)
        {
            _iterationService = service;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private readonly IterationService _iterationService;


        [HttpGet]
        public async Task<IActionResult> GetIterationStatus()
        {
            var result = await _iterationService.GetIterationStatusAsync(UserId);

            return StatusCode(0, result.ErrorCode);
        }

        [HttpGet("iterettes")]
        public async Task<IActionResult> GetAllIterettes()
        {
            var iterettes = await _iterationService.GetAllIterettesAsync(UserId);

            var iterettesDto = Mapper.MapToDto(iterettes);
            return Ok(iterettesDto);
        }


        [HttpPost]
        public async Task<IActionResult> StartIteration()
        {
            var result = await _iterationService.StartIterationAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "USER_NOT_FOUND" => NotFound(result.ErrorCode),
                    "ITERATION_NOT_FINISHED" => Conflict(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            return Ok();
        }

        [HttpPut("answer/{id:int}")]
        public async Task<IActionResult> SetIteretteAnswer(int id, string answer)
        {
            var result = await _iterationService.SetIteretteAnswerAsync(UserId, id, answer);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "ITERETTE_NOT_FOUND" => NotFound(result.ErrorCode),
                    "ITERATION_WAS_FINISHED" => BadRequest(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            var iteretteDto = Mapper.MapToDto(result.Value);
            return Ok(iteretteDto);
        }

        [HttpPost("finish")]
        public async Task<IActionResult> FinishIteration()
        {
            var result = await _iterationService.FinishIterationAsync(UserId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
