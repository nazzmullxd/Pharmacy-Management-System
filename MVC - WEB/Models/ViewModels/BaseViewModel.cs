namespace MVC_WEB.Models.ViewModels
{
    /// <summary>
    /// Base view model with common properties for all views
    /// </summary>
    public class BaseViewModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public Guid UserId { get; set; }
        
        public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
        public bool IsAuthenticated => !string.IsNullOrEmpty(UserEmail);
    }
}
