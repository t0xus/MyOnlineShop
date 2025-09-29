using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyOnlineShop.Pages
{
    public class logoutModel : PageModel
    {
        public void OnGet()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Optionally, you can also set a message to indicate successful logout
            TempData["Message"] = "You have been logged out successfully.";

            // Redirect to the login page or home page
            Response.Redirect("Index");
        }
    }
}
