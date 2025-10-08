using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using MyOnlineShop.Models;

namespace MyOnlineShop.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        private readonly ILogger<IndexModel> _logger;

        private readonly myonlineshopContext _context;

        public Boolean not_confirmed { get; set; } = false;
        public Boolean wrong_credentials { get; set; } = false;

        public IndexModel(ILogger<IndexModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                Response.Redirect("Items");
            }
        }

        public async Task<IActionResult> OnPost()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Username and Password are required.");
                return Page();
            }
            else
            {
                string hashed = ComputeSha256Hash(Password);

                Customer customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == Username && c.PwHash == hashed);

                if (customer != null && customer.EmailConfirmed == true)
                {
                    // Login erfolgreich -> Session setzen
                    HttpContext.Session.SetInt32("UserId", customer.Id);
                    HttpContext.Session.SetString("Username", customer.Username);
                    HttpContext.Session.SetString("IsAdmin", customer.IsAdmin.ToString());

                    // Here you would typically validate the user credentials against a database or service.
                    // For simplicity, we will just log the credentials and redirect to a success page.

                    _logger.LogInformation($"User {Username} logged in successfully.");

                    // Redirect to a success page or another action
                    return RedirectToPage("Items");

                }
                else if (customer != null && customer.EmailConfirmed == false)
                {
                    not_confirmed = true;
                    
                    return Page();
                }
                else
                {
                    wrong_credentials = true;
                    return Page();
                }

                return Page();
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - gibt ein Byte-Array zurück
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Byte-Array in einen hexadezimalen String umwandeln
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
