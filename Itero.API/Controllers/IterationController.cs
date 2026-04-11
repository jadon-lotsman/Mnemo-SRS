using System.Security.Claims;
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
        public async Task<IActionResult> GetIteration()
        {
            var iteration = await _iterationService.GetIterationAsync(UserId);

            if (iteration == null) 
                return NotFound();

            return Ok(iteration);
        }

        [HttpGet("iterettes")]
        public async Task<IActionResult> GetAllIterettes()
        {
            var iterettes = await _iterationService.GetAllIterettesAsync(UserId);

            return Ok(iterettes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIteration()
        {
            var result = await _iterationService.CreateIterationAsync(UserId);

            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpPut("answer/{id:int}")]
        public async Task<IActionResult> SetIteretteAnswer(int id, string answer)
        {
            bool success = await _iterationService.SetIteretteAnswerAsync(UserId, id, answer);

            if (!success)
                return BadRequest();

            return Ok();
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
