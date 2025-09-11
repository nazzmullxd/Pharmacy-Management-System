using Business.DTO;
using Database.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Reports
{
    public class AntibioticModel : PageModel
    {
        private readonly IAntibioticLogRepository _antibioticLogRepository;

        public AntibioticModel(IAntibioticLogRepository antibioticLogRepository)
        {
            _antibioticLogRepository = antibioticLogRepository;
        }

        public IEnumerable<AntibioticLogDTO> AntibioticLogs { get; set; } = Enumerable.Empty<AntibioticLogDTO>();

        public async Task OnGet()
        {
            // Note: This would need to be implemented in the repository/service layer
            // For now, return empty collection as placeholder
            AntibioticLogs = new List<AntibioticLogDTO>();
        }
    }
}


