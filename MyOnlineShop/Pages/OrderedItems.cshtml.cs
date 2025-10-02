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

        public List<CountOrderedCommission> countOrderedCommission = new List<CountOrderedCommission>();

        public Boolean isAdmin { get; set; } = false;
        [BindProperty]
        public string item_id { get; set; }
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

            //Hole alle Bestellungen wo die deal Objekte erledigt (done) sind und shipped gleichzeitig false ist
            var orderedItems = _context.ShoppingCarts
                .Where(c => _context.Deals.Any(d => d.Id == c.IdDl && d.Done == true && d.Shipped == false))
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

            //Hole alle Dealobjekte, die erledigt (done) sind und gleichzeitig nicht shipped sind
            var orderedCommissions = _context.Deals
                .Where(d => d.Done == true && d.Shipped == false)
                .ToList();

            foreach (var commission in orderedCommissions)
            {
                //Hole alle ShoppingCart Objekte zu der jeweiligen Kommission
                var commissionItems = _context.ShoppingCarts
                    .Where(sc => sc.IdDl == commission.Id)
                    .ToList();

                //Erstelle eine Liste von CountOrderedCommission
                foreach (var item in commissionItems)
                {
                    countOrderedCommission.Add(new CountOrderedCommission
                    {
                        id = commission.Id,
                        total = commission.Total.Value,
                        last_update = commission.LastUpdate,
                        done = commission.Done,
                        shipped = commission.Shipped,
                        countOrderedCommissionItems = _context.ShoppingCarts
                            .Where(sc => sc.IdDl == commission.Id)
                            .Join(_context.Items,
                                  sc => sc.IdIt,
                                  i => i.Id,
                                  (sc, i) => new CountOrderedCommissionItems
                                  {
                                      id = i.Id,
                                      name = i.Name,
                                      price = i.Price,
                                      anzahl = _context.ShoppingCarts.Count(c => c.IdIt == i.Id && c.IdDl == commission.Id)
                                  })
                            .ToList()
                    });
                }

            }
        }

        public async Task<IActionResult> OnPost()
        {
            if (!String.IsNullOrEmpty(item_id))
            {
                //Setze das jeweilige Deal Objekt auf shipped = true
                var deal = _context.Deals
                    .FirstOrDefault(d => d.Id == Convert.ToInt32(item_id));
                if (deal != null)
                {
                    deal.Shipped = true;
                    deal.LastUpdate = DateTime.Now;

                    //Speichere die Änderungen
                    await _context.SaveChangesAsync();

                    
                }

                
            }
            OnGet();
            return Page();
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

        public class CountOrderedCommission
        {
            public int id { get; set; }
            public double total { get; set; }
            public DateTime? last_update { get; set; }
            public Boolean? done { get; set; }
            public Boolean? shipped { get; set; }

            public List<CountOrderedCommissionItems> countOrderedCommissionItems = new List<CountOrderedCommissionItems>();

        }

        public class CountOrderedCommissionItems
        {
            public int id { get; set; }
            public string name { get; set; }
            public double? price { get; set; }
            public int anzahl { get; set; }
        }
    }
}


