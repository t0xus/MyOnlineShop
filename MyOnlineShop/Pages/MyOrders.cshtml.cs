using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyOnlineShop.Models;
using System.Linq;

namespace MyOnlineShop.Pages
{

    public class Orders
    { 
        public int id { get; set; }
        public DateTime? order_date { get; set; }
        public double? total { get; set;}

        public Boolean? shipped { get; set; }

        public List<OrderedItems> orderedItems = new List<OrderedItems>();

    }

    public class OrderedItems
    { 
        public string name { get; set; }
        public int? quantity { get; set; }
        public double? price { get; set; }
    }

    public class MyOrdersModel : PageModel
    {
        private readonly myonlineshopContext _context;

        private readonly ILogger<MyOrdersModel> _logger;
        public List<Orders> orders = new List<Orders>();
        [BindProperty]
        public int deal_id { get; set; }
        public MyOrdersModel(ILogger<MyOrdersModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            //Hole die UserId aus der Session
            var userId = HttpContext.Session.GetInt32("UserId");

            //Hole alle Bestellungen wo die deal Objekte erledigt (done) sind vom Kunden
            var deals = _context.Deals
                .Where(d => _context.ShoppingCarts.Any(c => d.Id == c.IdDl && d.Done == true && c.IdCs == userId))
                .ToList();

            foreach (var deal in deals)
            {
                var order = new Orders
                {
                    id = deal.Id,
                    order_date = deal.LastUpdate,
                    total = deal.Total,
                    shipped = deal.Shipped
                };

                // Hole alle ShoppingCart-Einträge, die zu diesem Deal gehören
                var shopping_carts = _context.ShoppingCarts
                    .Where(a => a.IdDl == deal.Id)
                    .ToList();

                // ✅ Gruppiere nach Artikel-Id (IdIt), um doppelte Artikel zusammenzufassen
                var groupedCarts = shopping_carts
                    .GroupBy(c => c.IdIt)
                    .ToList();

                List<OrderedItems> oi = new List<OrderedItems>();

                foreach (var group in groupedCarts)
                {
                    // Hole den Artikel selbst
                    var item = _context.Items.FirstOrDefault(a => a.Id == group.Key);
                    if (item == null)
                        continue;

                    // Berechne die Anzahl (wie oft kommt der Artikel im Deal vor)
                    int quantity = group.Count();

                    // Optional: Berechne den Gesamtpreis dieses Artikels (wenn gewünscht)
                    double priceTotal = (double)(item.Price * quantity);

                    // Erstelle den Eintrag für die Bestellübersicht
                    OrderedItems orderedItem = new OrderedItems
                    {
                        name = item.Name,
                        price = priceTotal,
                        quantity = quantity
                    };

                    oi.Add(orderedItem);
                }

                order.orderedItems = oi;
                orders.Add(order);
            }







            //Hole alle Shopping_Cart Einträge des Kunden
            //var dealIds = deals.Select(d => d.Id).ToList();
            //var shoppingCarts = _context.ShoppingCarts
            //    .Where(c => c.IdCs == userId && dealIds.Contains((int)c.IdDl))
            //    .ToList();

            ////Gruppiere die Bestellungen nach Deal (Bestellung)
            //var groupedOrders = shoppingCarts
            //    .GroupBy(c => c.IdDl)
            //    .ToList();

            //foreach (var orderGroup in groupedOrders)
            //    {
            //    var order = new Orders();
            //    var firstCartItem = orderGroup.FirstOrDefault();
            //    if (firstCartItem != null)
            //    {
            //        var deal = deals.FirstOrDefault(d => d.Id == firstCartItem.IdDl);
            //        //order.order_date = deal?.OrderDate;
            //    }

            //    double total = 0;
            //    foreach (var cartItem in orderGroup)
            //    {
            //        var orderedItem = new OrderedItems
            //        {
            //            //name = cartItem.IdItNavigation?.Name,
            //            //quantity = cartItem.Quantity,
            //            //price = cartItem.Price
            //        };
            //        //total += cartItem.Price * cartItem.Quantity;
            //        order.orderedItems.Add(orderedItem);
            //    }
            //    order.total = total;
            //    orders.Add(order);
            //}













        }

        public async Task<IActionResult> OnPost()
        {
            //Hole das aktuelle Deal Objekt
            var deal = _context.Deals
                .FirstOrDefault(d => d.Id == deal_id);
            
            //Lösche alle ShoppingCart Einträge die zu diesem Deal gehören
            var cartItems = _context.ShoppingCarts
                .Where(c => c.IdDl == deal_id);
            _context.ShoppingCarts.RemoveRange(cartItems);

            //Lösche das Deal Objekt
            if (deal != null)
            {
                _context.Deals.Remove(deal);
            }
            await _context.SaveChangesAsync();

            OnGet();
            return Page();
        }
    }
}
