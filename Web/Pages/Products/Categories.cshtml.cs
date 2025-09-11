using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Products
{
    public class CategoriesModel : PageModel
    {
        [BindProperty]
        public List<string> Categories { get; set; } = new List<string>();

        public void OnGet() { }

        public IActionResult OnPostAdd(string category)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                Categories.Add(category);
            }
            return Page();
        }
    }
}


