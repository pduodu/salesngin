namespace salesngin.ViewModels
{
    public class ModuleViewModel : BaseViewModel
    {
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        public int RoleId { get; set; }
        public int ModuleId { get; set; }
        public virtual ApplicationRole Role { get; set; }
        public virtual Module Module { get; set; }

        public virtual List<Module> Modules { get; set; }
        public virtual List<RoleModule> RoleModules { get; set; }
        public virtual List<ApplicationRole> Roles { get; set; }
        public virtual List<ApplicationUser> Users { get; set; }
    }
}
