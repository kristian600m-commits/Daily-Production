using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DailyProduction.Models;
using IbasAPI.Services;

namespace IbasAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DailyProductionController : ControllerBase
    {
        private readonly ProductionService _productionService;
        private readonly ILogger<DailyProductionController> _logger;

        // Vi modtager ProductionService via Dependency Injection
        public DailyProductionController(ILogger<DailyProductionController> logger, ProductionService productionService)
        {
            _logger = logger;
            _productionService = productionService;
        }

        [HttpGet]
        public async Task<IEnumerable<DailyProductionDTO>> Get()
        {
            // Erstattet den hardcoded liste med hentning fra Azure Table Storage!
            return await _productionService.GetDailyProductionsAsync();
        }
    }
}