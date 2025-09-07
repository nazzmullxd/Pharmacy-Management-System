using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Products
{
    public class EditModel : PageModel
    {
        [FromRoute]
        public string Id { get; set; } = string.Empty;

        public void OnGet(string id)
        {
            Id = id;
        }
    }
}


