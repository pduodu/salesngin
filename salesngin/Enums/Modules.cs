namespace salesngin.Enums
{
    public static class SystemModules
    {


        public static readonly List<(int, string, string)> Modules =
        [
            (1, ConstantModules.Profile_Module, "User Profile"),
            (2, ConstantModules.User_Module, "User Management"),
            (3, ConstantModules.Employee_Module, "Employee Management"),
            (4, ConstantModules.Category_Module, "Categories"),
            (5, ConstantModules.System_Settings, "System Settings"),
            (6, ConstantModules.Items_Module, "Items Management"),
            (7, ConstantModules.Items_Requisition_Module, "Item Requisition"),
            (8, ConstantModules.Inventory_Module, "Inventory"),
            (9, ConstantModules.Sales_Module, "Sales Management"),
            (10, ConstantModules.Stock_Module, "Stock Management"),
            (11, ConstantModules.Purchase_Module, "Purchases Management"),
            (12, ConstantModules.Orders_Module, "Order Management"),
            (13, ConstantModules.Facility_Module, "Facility Management"),
            (14, ConstantModules.Inspections_Module, "Inspections Management"),
            (15, ConstantModules.Faults_Module, "Fault Management"),
            (16, ConstantModules.Reporting_Module, "Reports Generation"),
        ];
    }

    public static class ConstantModules
    {
        public const string Profile_Module = "Profile";
        public const string User_Module = "User";
        public const string Employee_Module = "Employee";

        public const string Category_Module = "Category";
        public const string System_Settings = "Settings";

        public const string Items_Module = "Items";
        public const string Items_Requisition_Module = "ItemRequisition";
        public const string Inventory_Module = "Inventory";

        public const string Sales_Module = "Sales";
        public const string Stock_Module = "Stock";
        public const string Purchase_Module = "Purchases";
        public const string Orders_Module = "Orders";

        public const string Facility_Module = "Facility";
        public const string Inspections_Module = "Inspections";
        public const string Faults_Module = "Faults";

        public const string Reporting_Module = "Report Generation";

    }


}
