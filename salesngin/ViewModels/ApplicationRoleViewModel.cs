namespace salesngin.ViewModels
{
    public class ApplicationRoleViewModel
    {

        public ApplicationRole Role { get; set; }
        public IEnumerable<ApplicationRole> Roles { get; set; }


        public bool IsMaster { get; set; }
        public int? UserId { get; set; }
        public string RoleName { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<ApplicationUser> Users { get; set; }
    }
}
