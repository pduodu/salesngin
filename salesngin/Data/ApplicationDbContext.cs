using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace salesngin.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    //Purpose: 
    // Fixed assets are used to support business operations, 
    // while non-fixed assets are held for sale or consumption.

    //begin-----------Application 
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Title> Titles { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Company> Company { get; set; }
    public DbSet<CronJob> CronJobs { get; set; }
    public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<RoleModule> RoleModules { get; set; }
    public DbSet<ModulePermission> ModulePermissions { get; set; }

    public DbSet<ReportViewModel> Report { get; set; }
    //end-----------Application 


    //begin--------Organizational Models
    public DbSet<Unit> Units { get; set; }
    public DbSet<EmployeeType> EmployeeTypes { get; set; }
    public DbSet<Customer> Customers { get; set; }

    //end--------Organizational Models

    public DbSet<Models.Item> Items { get; set; }
    public DbSet<ExpiredItem> ExpiredItems { get; set; }
    public DbSet<DefectiveItem> DefectiveItems { get; set; }
    public DbSet<Inventory> Inventory { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderComment> OrderComments { get; set; }

    
    public DbSet<Sale> Sales { get; set; }
    public DbSet<Payment> Payments { get; set; }

    public DbSet<Refund> Refunds { get; set; }
    public DbSet<RefundItem> RefundItems { get; set; }
    public DbSet<RefundPayment> RefundPayments { get; set; }
    


    public DbSet<Models.Location> Locations { get; set; }
    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestItem> RequestItems { get; set; }
    public DbSet<RequestComment> RequestComments { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<StockItem> StockItems { get; set; }
    public DbSet<Fault> Faults { get; set; }
    public DbSet<FaultAction> FaultActions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define the relationship between ApplicationUser and EmployeeType
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(a => a.EmployeeType)         // One ApplicationUser has one EmployeeType
            .WithMany(e => e.ApplicationUsers)   // One EmployeeType can have many ApplicationUsers
            .HasForeignKey(a => a.EmployeeTypeId) // FK in ApplicationUser
            .OnDelete(DeleteBehavior.Restrict);  // Adjust delete behavior as needed

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(a => a.Unit)         // One ApplicationUser has one EmployeeType
            .WithMany(e => e.ApplicationUsers)   // One EmployeeType can have many ApplicationUsers
            .HasForeignKey(a => a.UnitId) // FK in ApplicationUser
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }

}

