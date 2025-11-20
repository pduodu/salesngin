namespace salesngin.ViewModels
{
    public class RoleViewModel : BaseViewModel
    {

        public ApplicationRole Role { get; set; }

        public IEnumerable<ApplicationRole> Roles { get; set; }
        public virtual List<ApplicationUser> Users { get; set; }
    }
}
