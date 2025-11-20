namespace salesngin.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherName { get; set; }
        public string StaffNumber { get; set; }
        public string Email { get; set; }
        public string UserFullName { get; set; }
        public string PhoneNumber { get; set; }
        public string UserPhoto { get; set; }

        //For Role
        public bool IsMaster { get; set; }
        //public Employee 
        //public string SelectedRoleId { get; set; }

        public List<ApplicationRole> Roles { get; set; }
        public List<UserRoles> UserRoles { get; set; }
        public ICollection<ApplicationRole> ApplicationRoles { get; set; }
        //public ICollection<UserRoleLevel> UserRoleLevels { get; set; }
    }
}
