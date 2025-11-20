namespace salesngin.ViewModels
{
    public class UsersRoleViewModel
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string RoleName { get; set; }

        public List<Unit> Units { get; set; }


    }
}
