using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyOnlineShop.Models;

namespace MyOnlineShop.Pages
{
    public class EditItemsCategoryModel : PageModel
    {

        private readonly myonlineshopContext _context;

        private readonly ILogger<EditItemsCategoryModel> _logger;

        [BindProperty]
        public string CategoryName { get; set; }
        [BindProperty]
        public string save { get; set; }
        [BindProperty]
        public string SelectedCategory { get; set; }
        [BindProperty]
        public bool DeleteCategory { get; set; }
        public List<SelectListItem> Categorys { get; set; }

        public EditItemsCategoryModel(ILogger<EditItemsCategoryModel> logger, myonlineshopContext context)
        {
            _logger = logger;
            _context = context;
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
                Text = "<<New Entry>>"
            };

            Categorys = new List<SelectListItem>();
            Categorys.Add(FirstEntryPoint);
            Categorys.AddRange(CategoryOptions);
        }

        public void OnGet()
        {
            fillCategorys();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!string.IsNullOrEmpty(save))
            {
                if(DeleteCategory == true)
                {
                    //Hole die Kategorie, die gelöscht werden soll
                    var category = _context.ItemCategories
                        .FirstOrDefault(c => c.Id == Convert.ToInt32(SelectedCategory));
                    if (category != null)
                        {
                        _context.ItemCategories.Remove(category);

                        try
                        {
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Item category deleted successfully.");
                        }
                        catch (DbUpdateException ex)
                        {
                            _logger.LogError(ex, "An error occurred while deleting the item category.");
                            ModelState.AddModelError(string.Empty, "An error occurred while deleting the category. Please try again.");
                            //return Page();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Category not found.");
                        //return Page();
                    }
                }
                else if (SelectedCategory == "0")
                {
                    var newContent = new ItemCategory
                    {
                        Name = CategoryName
                    };

                    _context.ItemCategories.Add(newContent);

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("New item category added successfully.");

                        CategoryName = string.Empty; // Clear the input field after successful save
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError(ex, "An error occurred while adding a new item category.");
                        ModelState.AddModelError(string.Empty, "An error occurred while saving the category. Please try again.");
                        //return Page();
                    }

                }
                else if(SelectedCategory != "0")
                {
                    //Hole die Kategorie, die editiert werden soll
                    var category = _context.ItemCategories
                        .FirstOrDefault(c => c.Id == Convert.ToInt32(SelectedCategory));
                    if (category != null)
                    {
                        category.Name = CategoryName;

                        _context.ItemCategories.Update(category);

                        try
                        {
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Item category updated successfully.");
                        }
                        catch (DbUpdateException ex)
                        {
                            _logger.LogError(ex, "An error occurred while updating the item category.");
                            ModelState.AddModelError(string.Empty, "An error occurred while saving the category. Please try again.");
                            //return Page();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Category not found.");
                        //return Page();
                    }

                }

                //return Page();
            }

            fillCategorys();

            return Page();
        }
    }
}
