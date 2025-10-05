using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyOnlineShop.Models;

namespace MyOnlineShop.Pages
{

    public class CountCheckout
    {

        public string product { get; set; }
        public int count { get; set; }
        public double price_total{ get; set; }
    }

    public class checkoutModel : PageModel
    {
        private readonly ILogger<checkoutModel> _logger;
        private readonly myonlineshopContext _context;

        public List <CountCheckout> checkouts  = new List<CountCheckout>();
        //public List <CountCheckout> checkouts2 = new List<CountCheckout>();
        [BindProperty]
        public string buy_sub { get; set; }

        public checkoutModel(ILogger<checkoutModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            //Hole den aktuellen Kunden
            int? customerId = HttpContext.Session.GetInt32("UserId");

            //Hole alle Produkte im Warenkorb des Kunden
            var shoppingCarts = _context.ShoppingCarts
                .Where(c => c.IdCs == customerId && _context.Deals.Any(d => d.Id == c.IdDl && d.Done == false))
                .ToList();



            //foreach (var cart in shoppingCarts)
            //{
            //    //Hole das Produkt
            //    var item = _context.Items.FirstOrDefault(i => i.Id == cart.IdIt);
            //    if (item != null)
            //    {
            //        //Berechne den Gesamtpreis für das Produkt
            //        double priceTotal = cart.Price.Value;

            //        //Zähle die Anzahl der mit gleicher IdIt im Warenkorb
            //        var count = shoppingCarts.Count(c => c.IdIt == cart.IdIt && c.IdCs == customerId && _context.Deals.Any(d => d.Id == c.IdDl && d.Done == false));

            //        checkouts.Add(new CountCheckout
            //        {
            //            product = item.Name,
            //            count = count,
            //            price_total = priceTotal * count
            //        });


            //    }
            //}

            var groupedCarts = shoppingCarts
                .GroupBy(c => c.IdIt)
                .ToList();

            foreach (var group in groupedCarts)
            {
                // Hole das Produkt einmal
                var item = _context.Items.FirstOrDefault(i => i.Id == group.Key);
                if (item != null)
                {
                    double priceTotal = item.Price.Value;
                    int count = group.Count(); // Anzahl in dieser Gruppe

                    checkouts.Add(new CountCheckout
                    {
                        product = item.Name,
                        count = count,
                        price_total = priceTotal * count
                    });
                }
            }


        }

        public async Task<IActionResult> OnPost()
        {
            if (!string.IsNullOrEmpty(buy_sub))
            {
                //Hole den aktuellen Kunden
                int? customerId = HttpContext.Session.GetInt32("UserId");

                

                //Hole das richtige ShoppingCart Objekt des Kunden
                var shoppingCart = _context.ShoppingCarts
                    .Where(c => c.IdCs == customerId && _context.Deals.Any(d => d.Id == c.IdDl && d.Done == false))
                    .ToList();

                //Hole das richtige Deal Objekt des Kunden
                var deal = _context.Deals
                    .FirstOrDefault(d => d.Id == shoppingCart[0].IdDl);

                //Setzte Done auf true
                if (deal != null) {
                    deal.Done = true;
                    deal.LastUpdate = DateTime.Now;

                    //Speichere die Änderungen
                    _context.Deals.Update(deal);
                    await _context.SaveChangesAsync();

                    //Leere den Warenkorb des Kunden
                    //_context.ShoppingCarts.RemoveRange(shoppingCart);
                    //await _context.SaveChangesAsync();

                    //Redirect to success page
                    //return RedirectToPage("Items");
                    TempData["Message"] = "Kauf der Bestellung " + deal.Id + " erfolgreich abgeschlossen!";
                    return RedirectToPage("MyOrders");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Deal not found.");
                    return Page();
                }
                

            }
            else
            {
                return Page();
            }
                
        }
    }
}
