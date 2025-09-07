using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace Web.Pages.Products
{
    public class DetailsModel : PageModel
    {
        [FromRoute]
        public string Id { get; set; } = string.Empty;

        public void OnGet(string id)
        {
            Id = id;
        }
    }
}


