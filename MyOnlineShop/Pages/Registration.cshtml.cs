using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyOnlineShop.Models;
using System.Text;
using System.Security.Cryptography;

namespace MyOnlineShop.Pages
{
    public class RegistrationModel : PageModel
    {

        private readonly myonlineshopContext _context;
        private readonly ILogger<RegistrationModel> _logger;

        [BindProperty]
        public string first_name { get; set; }
        [BindProperty]
        public string last_name { get; set; }
        [BindProperty]
        public string street { get; set; }
        [BindProperty]
        public string zip { get; set; }
        [BindProperty]
        public string place { get; set; }
        [BindProperty]
        public string username { get; set; }
        [BindProperty]
        public string password { get; set; }
        [BindProperty]
        public string email { get; set; }
        [BindProperty]
        public string pw_strength { get; set; }

        public string wrong_fields { get; set; } = "";
        public RegistrationModel(ILogger<RegistrationModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            

        }

        public async Task<IActionResult> OnPost()
        {
            
            // Validate input fields
            wrong_fields = "";
            if (string.IsNullOrWhiteSpace(first_name))
            {
                wrong_fields += "First name is required. <br />";
            }
            if (string.IsNullOrWhiteSpace(last_name))
            {
                wrong_fields += "Last name is required. <br />";
            }
            if (string.IsNullOrWhiteSpace(street))
            {
                wrong_fields += "Street is required. <br />";
            }
            if (string.IsNullOrWhiteSpace(zip))
            {
                wrong_fields += "ZIP code is required. <br />";
            }
            if (string.IsNullOrWhiteSpace(place))
            {
                wrong_fields += "Place is required. <br />";
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                wrong_fields += "Username is required. <br />";
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                wrong_fields += "Password is required. <br />";
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                wrong_fields += "Email is required. <br />";
            }
            int pw_strength_int;
            //Prüfe Passwortstärke
            if (pw_strength is null)
            {
                pw_strength_int = 0;
            }
            else
            {
                pw_strength_int = int.Parse(pw_strength);
            }
            
            if (pw_strength_int < 3)
            {
                wrong_fields += "Password is too weak. Please choose a stronger one. <br />";
            }

            //Prüfe ob User bereits existiert
            var user_exists = _context.Customers
                .Where(c => c.Username == username)
                .Any();
            if (user_exists)
                {
                wrong_fields += "Username already exists. Please choose another one. <br />";
            }

            //Prüfe ob Email bereits existiert
            var email_exists = _context.Customers
                .Where(c => c.Email == email)
                .Any();
            if (email_exists)
                {
                wrong_fields += "Email already exists. Please choose another one. <br />";
            }

            if (!string.IsNullOrEmpty(wrong_fields))
            {
                return Page();
            }

            //Füge neuen User hinzu
            var temp_customer = new Customer();
            temp_customer.FirstName = first_name;
            temp_customer.LastName = last_name;
            temp_customer.Street = street;
            temp_customer.Zip = zip;
            temp_customer.Place = place;
            temp_customer.Username = username;
            temp_customer.PwHash = ComputeSha256Hash(password);
            temp_customer.Email = email;
            temp_customer.IsAdmin = false;
            temp_customer.EmailConfirmed = false;

            _context.Customers.Add(temp_customer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Registration successfully completed! <br />Please log in.";
            //OnGet();
            return RedirectToPage("Index");
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
