using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MyOnlineShop.Models;

namespace MyOnlineShop.Pages
{
    public class CountItems
    {

       public int Count { get; set; }
       public int id_item { get; set; }
    }

    public class PageItems
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class ItemsModel : PageModel
    {
        private readonly myonlineshopContext _context;

        private readonly ILogger<ItemsModel> _logger;

        public List<Item> Items { get; set; }

        public List<CountItems> CountItems { get; set; } = new List<CountItems>();

        [BindProperty(SupportsGet = true)]
        public string id_item { get; set; }
        [BindProperty(SupportsGet = true)]
        public string id_page { get; set; }
        [BindProperty(SupportsGet = true)]
        public string search_query { get; set; }
        [BindProperty(SupportsGet = true)]
        public string category_query { get; set; }

        [BindProperty]
        public string item_sub_id { get; set; }

        [BindProperty]
        public string item_entf_id { get; set; }
        [BindProperty]
        public string item_total_sum { get; set; }
        public ItemsModel(ILogger<ItemsModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
        }
        public int totalItems;

        public List<SelectListItem> Categorys { get; set; }

        public void OnGet()
        {
            fillCategorys();
            //Items = _context.Items
            //    .ToList();

            //Weise der Variable Items die Einträge 10 bis 20 aus der Tabelle Items zu
            //Items = _context.Items
            //    .Where(i => string.IsNullOrEmpty(search_query) || i.Name.Contains(search_query) || i.Description.Contains(search_query))
            //    .Skip(0)
            //    .Take(9)
            //    .ToList();
            int? categoryId = string.IsNullOrEmpty(category_query) ? null : int.Parse(category_query);

            if(categoryId == 0)
            {
                categoryId = null;
            }

            Items = _context.Items
                .Where(i =>
                    (string.IsNullOrEmpty(search_query) ||
                     i.Name.Contains(search_query) ||
                     i.Description.Contains(search_query))
                    &&
                    (!categoryId.HasValue || i.IdIc == categoryId.Value)
                )
                .Skip(0)
                .Take(9)
                .ToList();

            int page;
            int skip = 0;
            if (!string.IsNullOrEmpty(id_page))
            {
                page = int.Parse(id_page);
                skip = (page - 1) * 9;
                
                //Items = _context.Items
                //    .Where(i => string.IsNullOrEmpty(search_query) || i.Name.Contains(search_query) || i.Description.Contains(search_query))
                //    .Skip(skip)
                //    .Take(9)
                //    .ToList();
                Items = _context.Items
                    .Where(i =>
                        (string.IsNullOrEmpty(search_query) ||
                         i.Name.Contains(search_query) ||
                         i.Description.Contains(search_query))
                        &&
                        (!categoryId.HasValue || i.IdIc == categoryId.Value)
                    )
                    .Skip(skip)
                    .Take(9)
                    .ToList();
            }

            //Zähle wie viele Einträge es in der Tabelle Items gibt
            //double totalItemsDouble = _context.Items
            //    .Where(i => string.IsNullOrEmpty(search_query) || i.Name.Contains(search_query) || i.Description.Contains(search_query))
            //    .Count();
            double totalItemsDouble = _context.Items
                .Where(i =>
                    (string.IsNullOrEmpty(search_query) ||
                     i.Name.Contains(search_query) ||
                     i.Description.Contains(search_query))
                    &&
                    (!categoryId.HasValue || i.IdIc == categoryId.Value)
                )
                .Count();
            totalItemsDouble = totalItemsDouble / 9;

            //Runde auf die nächste ganze Zahl auf und konvertiere in int
            //totalItemsDouble = ;

            totalItems = Convert.ToInt32(Math.Ceiling(totalItemsDouble));

            foreach (var item in Items)
            {                 var count = _context.ShoppingCarts
                    .Count(c => c.IdIt == item.Id && c.IdCs == HttpContext.Session.GetInt32("UserId") && _context.Deals.Any(d => d.Id == c.IdDl && d.Done == false));

                CountItems.Add(new CountItems
                {
                    Count = count,
                    id_item = item.Id
                });
            }

            item_total_sum = _context.ShoppingCarts
                .Where(c => c.IdCs == HttpContext.Session.GetInt32("UserId") && _context.Deals.Any(d => d.Id == c.IdDl && d.Done == false))
                .Sum(c => c.Price)
                .ToString(); // Formatieren auf 2 Dezimalstellen



            if (!string.IsNullOrEmpty(id_item))
            {
                //Hole den aktuellen Kunden
                var customer = _context.Customers
                    .FirstOrDefault(c => c.Id == HttpContext.Session.GetInt32("UserId"));

                //Finde heraus ob es schon einen Deal Datensatz gibt, der noch nicht abgeschlossen ist
                var q = from c in _context.Customers join d in _context.ShoppingCarts
                        on c.Id equals d.IdCs
                        join e in _context.Deals
                        on d.IdDl equals e.Id
                where c.Id == customer.Id && e.Done == false
                select e;

                int id_deal = 0;

                if(q.Count() == 0)
                {
                    var addDeal = new Deal
                    {
                        Total = 0.0,
                        LastUpdate = DateTime.Now,
                        Done = false
                    };

                    _context.Deals.Add(addDeal);
                    _context.SaveChanges();
                    id_deal = addDeal.Id;  //Neuer Deal Datensatz wurde erstellt und gespeichert
                }
                else
                {                     //Es gibt einen Datensatz, der noch nicht abgeschlossen ist
                    id_deal = q.FirstOrDefault().Id;
                }

                var item = _context.Items
                    .FirstOrDefault(i => i.Id == int.Parse(id_item));

                var addItem = new ShoppingCart
                {
                    Price = item.Price,
                    DateAdded = DateTime.Now,
                    IdDl = id_deal, //ID des Deals, zu dem der Artikel gehört
                    IdCs = HttpContext.Session.GetInt32("UserId"),
                    IdIt = item.Id
                };

                _context.ShoppingCarts.Add(addItem);

                //Aktualisiere den Deal, um den Gesamtpreis zu berechnen
                var deal = _context.Deals
                    .FirstOrDefault(d => d.Id == id_deal);
                if (deal != null)
                    {
                    deal.Total += item.Price;
                    deal.LastUpdate = DateTime.Now;
                }

                _context.Deals.Update(deal);
                //Speichere die Änderungen in der Datenbank
                _context.SaveChanges();
            }
        }


        public async Task<IActionResult> OnPost()
        {
            
            if(!string.IsNullOrEmpty(item_sub_id))
            {
                //Hole den aktuellen Kunden
                var customer = _context.Customers
                    .FirstOrDefault(c => c.Id == HttpContext.Session.GetInt32("UserId"));

                //Finde heraus ob es schon einen Deal Datensatz gibt, der noch nicht abgeschlossen ist
                var q = from c in _context.Customers
                        join d in _context.ShoppingCarts
                        on c.Id equals d.IdCs
                        join e in _context.Deals
                        on d.IdDl equals e.Id
                        where c.Id == customer.Id && e.Done == false
                        select e;

                int id_deal = 0;

                if (q.Count() == 0)
                {
                    var addDeal = new Deal
                    {
                        Total = 0.0,
                        LastUpdate = DateTime.Now,
                        Done = false
                    };

                    _context.Deals.Add(addDeal);
                    _context.SaveChanges();
                    id_deal = addDeal.Id;  //Neuer Deal Datensatz wurde erstellt und gespeichert
                }
                else
                {                     //Es gibt einen Datensatz, der noch nicht abgeschlossen ist
                    id_deal = q.FirstOrDefault().Id;
                }

                var item = _context.Items
                    .FirstOrDefault(i => i.Id == int.Parse(item_sub_id));

                var addItem = new ShoppingCart
                {
                    Price = item.Price,
                    DateAdded = DateTime.Now,
                    IdDl = id_deal, //ID des Deals, zu dem der Artikel gehört
                    IdCs = HttpContext.Session.GetInt32("UserId"),
                    IdIt = item.Id
                };

                _context.ShoppingCarts.Add(addItem);

                //Aktualisiere den Deal, um den Gesamtpreis zu berechnen
                var deal = _context.Deals
                    .FirstOrDefault(d => d.Id == id_deal);
                if (deal != null)
                {
                    deal.Total += item.Price;
                    deal.LastUpdate = DateTime.Now;
                }

                _context.Deals.Update(deal);
                //Speichere die Änderungen in der Datenbank
                _context.SaveChanges();
            }

            if (!string.IsNullOrEmpty(item_entf_id))
            {
                //Hole den aktuellen Kunden
                var customer = _context.Customers
                    .FirstOrDefault(c => c.Id == HttpContext.Session.GetInt32("UserId"));

                //Hole den aktuellsten shoping cart Eintrag, der entfernt werden soll
                var shoppingCartItem = _context.ShoppingCarts
                    .Where(c => c.IdCs == customer.Id && c.IdIt == int.Parse(item_entf_id))
                    .OrderByDescending(c => c.DateAdded)
                    .FirstOrDefault();

                if (shoppingCartItem != null)
                {
                    //Hole den Deal, zu dem der Artikel gehört
                    var deal = _context.Deals
                        .FirstOrDefault(d => d.Id == shoppingCartItem.IdDl);

                    if (deal != null)
                    {
                        //Reduziere den Gesamtpreis des Deals
                        deal.Total -= shoppingCartItem.Price;
                        deal.LastUpdate = DateTime.Now;

                        _context.Deals.Update(deal);
                    }

                    //Entferne den ShoppingCart Eintrag
                    _context.ShoppingCarts.Remove(shoppingCartItem);
                    _context.SaveChanges();
                }

                //Lösche den Dealdatensatz, wenn es keine referenz mehr zur shopping cart gibt
                var dealToDelete = _context.Deals
                    .FirstOrDefault(d => d.Id == shoppingCartItem.IdDl && !_context.ShoppingCarts.Any(sc => sc.IdDl == d.Id));

                if (dealToDelete != null)
                {
                    _context.Deals.Remove(dealToDelete);
                    _context.SaveChanges();
                }
            }
                OnGet();
            return Page();
        }

        private void fillCategorys()
        {
            var CategoryOptions = _context.ItemCategories.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = l.Name
            }).ToList();


            var FirstEntryPoint = new SelectListItem
            {
                Value = "0",
                Text = "Beliebig"
            };

            Categorys = new List<SelectListItem>();
            Categorys.Add(FirstEntryPoint);
            Categorys.AddRange(CategoryOptions);
        }
    }
}
