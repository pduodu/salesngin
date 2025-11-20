namespace salesngin.Enums
{
    public static class ConstantPermissions
    {
        public const string Create = "CREATE";
        public const string Read = "READ";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
        public const string Export = "EXPORT";
        public const string Configure = "CONFIGURE";
        public const string Approve = "APPROVE";
        public const string Appoint = "APPOINT";
        public const string Report = "REPORT";
    }

    public enum Perms
    {
        CREATE, READ, UPDATE, DELETE, EXPORT, CONFIGURE, APPROVE, APPOINT, REPORT
    }
}
