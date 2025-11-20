namespace salesngin.ViewModels
{
    public class RoleModuleViewModel : BaseViewModel
    {

        public int? RoleId { get; set; }
        public int ModuleId { get; set; }
        public int RoleModuleId { get; set; }
        public int? UserId { get; set; }

        public virtual RoleModule RoleModule { get; set; }
        public virtual ApplicationRole Role { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Module Module { get; set; }

        public virtual List<RoleModule> RoleModules { get; set; }
        public virtual List<Module> Modules { get; set; }
        public virtual List<ApplicationUser> Users { get; set; }
        public virtual List<ApplicationUser> UserList { get; set; }



    }
}
