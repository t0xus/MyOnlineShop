using Microsoft.AspNetCore.Mvc;
using MyOnlineShop.Models;
using MyOnlineShop.Pages;

namespace MyOnlineShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchSuggestions : ControllerBase
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly myonlineshopContext _context;
        public SearchSuggestions(ILogger<IndexModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult GetSuggestions([FromQuery] string term)
        {
            var allSuggestions = new List<string>();

            foreach (var content in _context.Items.ToList())
            {
                allSuggestions.Add(content.Name);
            }

            var result = allSuggestions
                .Where(s => s.StartsWith(term, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            return Ok(result);
        }
    }
}
