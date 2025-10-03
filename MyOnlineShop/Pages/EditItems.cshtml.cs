using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyOnlineShop.Models;

namespace MyOnlineShop.Pages
{
    public class EditItemsModel : PageModel
    {
        private readonly myonlineshopContext _context;

        private readonly ILogger<EditItemsModel> _logger;

        private readonly IWebHostEnvironment _environment;
        [BindProperty]
        public string name { get; set; }
        [BindProperty]
        public string price { get; set; }
        [BindProperty]
        public string delete { get; set; }
        [BindProperty]
        public string save { get; set; }
        [BindProperty]
        public string description { get; set; }
        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public Boolean isAdmin { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public string id_item { get; set; }
        [BindProperty]
        public string SelectedCategory { get; set; }

        public List<SelectListItem> Categorys { get; set; }

        public EditItemsModel(ILogger<EditItemsModel> logger, myonlineshopContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }


        public void OnGet()
        {
            //Hole aktuellen User
            var userId = HttpContext.Session.GetInt32("UserId");

            //isAdmin = _context.Customers.Where(c => c.Id == userId).Any();

            var q = _context.Customers
                .Where(c => c.Id == userId)
                .FirstOrDefault();
            
            isAdmin = q.IsAdmin.Value;


            if (!String.IsNullOrEmpty(id_item))
            {
                // Hole den Artikel, der editiert werden soll
                var item = _context.Items
                    .Where(i => i.Id == Convert.ToInt32(id_item))
                    .FirstOrDefault();

                if (item != null)
                {
                    name = item.Name;
                    price = item.Price.ToString();
                    description = item.Description;
                    SelectedCategory = item.IdIc.ToString();
                }
            }

            Categorys = _context.ItemCategories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();
        }

        public async Task<IActionResult> OnPost()
        {
            if(!String.IsNullOrEmpty(name) && String.IsNullOrEmpty(id_item))
            {   // Neuen Artikel anlegen
                if (ImageFile != null)
                {
                    // Speicherort im wwwroot/img Ordner
                    string uploadDir = Path.Combine(_environment.WebRootPath, "img");
                    Directory.CreateDirectory(uploadDir);

                    string filePath = Path.Combine(uploadDir, ImageFile.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }


                    Item item = new Item
                    {
                        Name = name,
                        Price = (double?)Convert.ToDecimal(price),
                        Description = description,
                        IdIc = Convert.ToInt32(SelectedCategory), // Assuming SelectedCategory is the ID of the category
                        Filename = ImageFile.FileName,
                        Filepath = "/img/"
                    };

                    _context.Items.Add(item);
                    await _context.SaveChangesAsync();
                }
                else
                {                     
                    Item item = new Item
                    {
                        Name = name,
                        Price = (double?)Convert.ToDecimal(price),
                        Description = description,
                        IdIc = Convert.ToInt32(SelectedCategory) // Assuming SelectedCategory is the ID of the category
                    };

                    _context.Items.Add(item);
                    await _context.SaveChangesAsync();
                }


                // Redirect to Items page after adding the item
                return RedirectToPage("Items");
            }
            else if(!String.IsNullOrEmpty(save))
            {
                // Artikel aktualisieren
                var item = _context.Items
                .Where(i => i.Id == Convert.ToInt32(id_item))
                .FirstOrDefault();

                if (ImageFile != null)
                {
                    // Speicherort im wwwroot/img Ordner
                    string uploadDir = Path.Combine(_environment.WebRootPath, "img");
                    Directory.CreateDirectory(uploadDir);

                    string filePath = Path.Combine(uploadDir, ImageFile.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    if (item != null)
                    {
                        item.Name = name;
                        item.Price = (double?)Convert.ToDecimal(price);
                        item.Description = description;
                        item.IdIc = Convert.ToInt32(SelectedCategory);
                        item.Filename = ImageFile.FileName;
                        item.Filepath = "/img/";

                        _context.Items.Update(item);
                        await _context.SaveChangesAsync();
                    }

                }
                else
                {
                    if (item != null)
                    {
                        item.Name = name;
                        item.Price = (double?)Convert.ToDecimal(price);
                        item.Description = description;
                        item.IdIc = Convert.ToInt32(SelectedCategory);

                        _context.Items.Update(item);
                        await _context.SaveChangesAsync();
                    }
                }


                // Redirect to Items page after updating the item
                return RedirectToPage("Items");

            }
            else if (!String.IsNullOrEmpty(delete))
            {
                //Artikel löschen
                var item = _context.Items
                    .Where(i => i.Id == Convert.ToInt32(id_item))
                    .FirstOrDefault();
                if (item != null)
                    {
                    _context.Items.Remove(item);
                    await _context.SaveChangesAsync();
                }
                // Redirect to Items page after deleting the item
                return RedirectToPage("Items");
            }

                return Page();
        }
    }
}
