using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyOnlineShop.Models;

namespace MyOnlineShop.Pages
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly myonlineshopContext _context;
        private readonly ILogger<ConfirmEmailModel> _logger;

        [BindProperty(SupportsGet = true)]
        public string security_stamp { get; set; }

        public Boolean email_confirmed { get; set; } = false;

        public ConfirmEmailModel(ILogger<ConfirmEmailModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
        }
        public void OnGet()
        {
            if (!String.IsNullOrEmpty(security_stamp))
            {
                var customer = _context.Customers
                .FirstOrDefault(c => c.SecurityStamp == security_stamp && c.EmailConfirmed == false);

                if (customer != null)
                {
                    customer.EmailConfirmed = true;
                    customer.SecurityStamp = null; // Sicherheitstoken ungültig machen
                    _context.SaveChanges();
                    email_confirmed = true;
                }
            }

        }
        public async Task<IActionResult> OnPost()
        {
            return Page();
        }
    }
}
