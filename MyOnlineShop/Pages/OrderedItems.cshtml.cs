using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyOnlineShop.Models;
using System.Text.RegularExpressions;



namespace MyOnlineShop.Pages
{



    public class OrderedItemsModel : PageModel
    {
        private readonly myonlineshopContext _context;

        private readonly ILogger<OrderedItemsModel> _logger;

        public List<CountOrderedCustomerItems> countOrderedCustomerItems = new List<CountOrderedCustomerItems>();

        public Boolean isAdmin { get; set; } = false;

        public OrderedItemsModel(ILogger<OrderedItemsModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            //isAdmin = _context.Customers.Where(c => c.Id == userId).Any();

            var q = _context.Customers
                .Where(c => c.Id == userId)
                .FirstOrDefault();

            isAdmin = q.IsAdmin.Value;

            //Hole alle Bestellungen wo die deal Objekte erledigt (done) sind
            var orderedItems = _context.ShoppingCarts
                .Where(c => _context.Deals.Any(d => d.Id == c.IdDl && d.Done == true))
                .ToList();

            // Erste Gruppierung: nach Kunde
            var groupedOrderedItems = orderedItems
                .GroupBy(cd => cd.IdCs) // Kunde
                .ToList();

            foreach (var customerGroup in groupedOrderedItems)
            {
                var countOrderedItems = new List<CountOrderedItems>();

                // Zweite Gruppierung: nach Produkt innerhalb des Kunden
                var productGroups = customerGroup
                    .GroupBy(c => c.IdIt) // Produkt
                    .ToList();

                foreach (var productGroup in productGroups)
                {
                    var item = _context.Items.FirstOrDefault(i => i.Id == productGroup.Key);
                    if (item != null)
                    {
                        countOrderedItems.Add(new CountOrderedItems
                        {
                            product = item.Name,
                            count = productGroup.Count(),
                            price_total = productGroup.Sum(c => c.Price)
                        });
                    }
                }

                countOrderedCustomerItems.Add(new CountOrderedCustomerItems
                {
                    IdCs = customerGroup.Key,
                    countOrderedItems = countOrderedItems,
                    CustomerName = _context.Customers
                        .Where(c => c.Id == customerGroup.Key)
                        .Select(c => c.LastName + ", " + c.FirstName)
                        .FirstOrDefault()
                });
            }

        }
    }


    public class CountOrderedCustomerItems
    {
        public List<CountOrderedItems> countOrderedItems = new List<CountOrderedItems>();

        public int? IdCs { get; set; }

        public string CustomerName { get; set; }
    }
    public class CountOrderedItems
    {

        public string product { get; set; }
        public int count { get; set; }
        public double? price_total { get; set; }
    }
}
