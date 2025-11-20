using System.Runtime.InteropServices;

namespace salesngin.Enums;

public static class GlobalConstants
{
    public static readonly List<(int MonthNumber, string MonthName)> Months =
    [
     (1, "January"),
     (2, "February"),
     (3, "March"),
     (4, "April"),
     (5, "May"),
     (6, "June"),
     (7, "July"),
     (8, "August"),
     (9, "September"),
     (10, "October"),
     (11, "November"),
     (12, "December")
    ];

    public static readonly List<(int, string)> UserStatuses =
    [
        (0, UserStatus.Inactive),
        (1, UserStatus.Active),
    ];

    public static readonly List<(int, string)> OrderStatuses =
    [
        (1, OrderStatus.Cancelled),
        (2, OrderStatus.Delivered),
        (3, OrderStatus.New),
        (4, OrderStatus.Paid),
        (5, OrderStatus.Preparing),
        (6, OrderStatus.Ready),
        (7, OrderStatus.Hold),
        (8, OrderStatus.Pending)
    ];

    public static readonly List<(int, string)> OrderTypes =
    [
        (1, OrderType.Wholesale),
        (2, OrderType.Retail),
    ];

    public static readonly List<(int Value, string Text)> ItemTypes =
    [
        (1, ItemType.Fixed),
            (2, ItemType.Operational),
        ];

    public static readonly List<(int Value, string Text)> ItemStatuses =
    [
        (0, ItemStatus.Available),
            (1, ItemStatus.Unavailable),
        ];

    public static readonly List<(int, string)> ProductStatuses =
    [
        (0, ProductStatus.Available),
        (1, ProductStatus.Unavailable),
    ];


    public static readonly List<(int, string)> PurchaseStatuses =
    [
        (0, PurchaseStatus.Pending),
        (1, PurchaseStatus.Received),
        (2, PurchaseStatus.Cancelled),
    ];

    public static readonly List<(int, string)> RequestStatuses =
    [
        (0, RequestStatus.New),
        (1, RequestStatus.Approved),
        (2, RequestStatus.Cancelled),
        (3, RequestStatus.Delivered),
    ];

    public static readonly List<(int, string)> StockStatuses =
    [
        (0, StockStatus.Available),
        (1, StockStatus.LowStock),
        (2, StockStatus.OutOfStock),
    ];

    public static readonly List<(int, string)> SalesStatuses =
    [
        (0, SalesStatus.Paid),
        (1, SalesStatus.Unpaid),
        (2, SalesStatus.Part),
    ];

    public static readonly List<(int, string)> SalesFrequencies =
    [
        (0, PurchaseFrequencyCategory.High),
        (1, PurchaseFrequencyCategory.Medium),
        (2, PurchaseFrequencyCategory.Low),
    ];


    public static readonly List<(int, string)> PaymentMethods =
    [
        (0, PaymentMethod.Cash),
        (1, PaymentMethod.Card),
        (2, PaymentMethod.MobileMoney),
        (3, PaymentMethod.Mixed),
    ];

    public static readonly List<(int, string)> RefundTypes =
       [
          (0, RefundType.Cash),
           (1, RefundType.MobileMoney),
           (2, RefundType.ProductExchange),
           (3, RefundType.Mixed), // For partial cash + partial mobile money
        ];

    public static readonly List<(int, string)> ProductConditions =
    [
       (0, ProductCondition.Good),
           (1, ProductCondition.Defective),
           (2, ProductCondition.Expired),
           (3, ProductCondition.Damaged),
           (4, ProductCondition.Used),
        ];

    public static readonly List<(int, string)> RefundReasons =
    [
       (0, RefundReason.CustomerRequest),
           (1, RefundReason.ProductDefect),
           (2, RefundReason.ExpiredProduct),
           (3, RefundReason.WrongItem),
           //(4, RefundReason.SizeIssue),
           (4, RefundReason.QualityIssue),
           (5, RefundReason.Other),
        ];

    public static readonly List<(int, string)> DefaultCategories =
    [
        // (1, DefaultCategory.Titles),
        (1, DefaultCategory.ItemCategory),
        (2, DefaultCategory.UnitOfMeasurement),
    ];

    public static readonly List<(int Value, string Text)> FixedItemStatuses =
    [
      (1, FixedItemStatus.Good),
          (2, FixedItemStatus.OutOfOrder),
          (3, FixedItemStatus.UnderMaintenance),
          (4, FixedItemStatus.UnderRepairs),
        ];

    public static readonly List<(int Value, string Text)> OperationalItemStatuses =
    [
      (1, OperationalItemStatus.Available),
          (2, OperationalItemStatus.Unavailable),
        ];

    public static readonly List<(int, string)> FaultStatuses =
    [
        (0, FaultStatus.Open),
            (1, FaultStatus.InProgress),
            (2, FaultStatus.Resolved),
            (3, FaultStatus.Closed),
   ];


}

public static class FaultStatus
{
    public const string Open = "Open";       // Blue
    public const string InProgress = "In-Progress"; // Orange
    public const string Resolved = "Resolved"; // Green
    public const string Closed = "Closed";     // Gray
}
public static class DefaultCategory
{
    // public const string Titles = "Titles";
    public const string ItemCategory = "Item Category";
    public const string UnitOfMeasurement = "Unit Of Measurement";
}
public static class UserStatus
{
    public const string Active = "Active"; //Green
    public const string Inactive = "Inactive"; //Red
}
public static class ItemType
{
    public const string Fixed = "Fixed Items"; //Green
    public const string Operational = "Operational Items"; //Red
}
public static class ItemStatus
{
    public const string Available = "Available"; //Green
    public const string Unavailable = "Unavailable"; //Red
}
public static class OperationalItemStatus
{
    public const string Available = "Available"; //Green
    public const string Unavailable = "Unavailable"; //Red
}
public static class FixedItemStatus
{
    public const string Good = "Good"; //The item is functional, with some signs of wear and tear
    public const string UnderMaintenance = "Under Maintenance"; //Red
    public const string OutOfOrder = "Out of Order"; //Red
    public const string UnderRepairs = "Under Repairs"; //The item is being repaired or refurbished
}
public static class ItemCondition
{
    public const string Good = "Available"; //Green
    public const string Damaged = "Unavailable"; //Red
    public const string NeedsRepair = "Needs Repair"; //Red
}
public static class RequestStatus
{
    public const string New = "New";
    public const string Approved = "Approved";
    public const string Cancelled = "Cancelled";
    public const string Delivered = "Delivered";
    public const string InProgress = "In-Progress";
}

public static class RequestItemStatus
{
    public const string New = "New"; //Blue
    public const string Ok = "Ok"; //Yellow
    public const string Updated = "Updated"; //Red
    public const string Delivered = "Delivered"; //Green
    public const string Paid = "Paid"; //Green
}
public static class PurchaseStatus
{
    public const string Pending = "Pending"; //Blue
    public const string Received = "Received"; //Green
    public const string Cancelled = "Cancelled"; //Red
}
public static class StockStatus
{
    public const string OutOfStock = "Out of Stock"; //Red
    public const string LowStock = "Low Stock"; //Yellow
    public const string Available = "Available"; //Green
}
public static class SalesStatus
{
    public const string Paid = "Paid"; //Red
    public const string Unpaid = "Unpaid"; //Green
    public const string Part = "Partial Payment"; //Blue
}
public static class PaymentMethod
{
    public const string Cash = "Cash"; 
    public const string Card = "Card"; 
    public const string MobileMoney = "MobileMoney"; 
    public const string Mixed = "Mixed"; 
}

public static class OrderType
{
    public const string Wholesale = "Wholesale";
    public const string Retail = "Retail";
}
public static class OrderStatus
{
    public const string New = "New";
    public const string Hold = "Hold";
    public const string Preparing = "Preparing";
    public const string Cancelled = "Cancelled";
    public const string Ready = "Ready";
    public const string Pending = "Pending";
    public const string Delivered = "Delivered";
    public const string Paid = "Paid";
}

public static class ProductStatus
{
    public const string Unavailable = "Unavailable"; 
    public const string Available = "Available"; 
}

public static class PurchaseFrequencyCategory
{
    public const string High = "High";
    public const string Medium = "Medium"; 
    public const string Low = "Low"; 
}

public static class RefundMethod
{
    public const string Cash = "Cash"; //Red
    public const string Product = "Product"; //Green
}


public static class RefundType
{
    public const string Cash = "Cash";
    public const string MobileMoney = "MobileMoney";
    public const string ProductExchange = "Exchange";
    public const string Mixed = "Mixed"; // For partial cash + partial mobile money
}

public static class RefundReason
{
    public const string CustomerRequest = "Customer Request";
    public const string ProductDefect = "Product Defect";
    public const string ExpiredProduct = "Expired Product";
    public const string WrongItem = "Wrong Item";
    public const string QualityIssue = "Quality Issue";
    public const string Other = "Other";
}

public static class RefundStatus
{
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public static class ProductCondition
{
    public const string Good = "Good";           // Return to inventory
    public const string Defective = "Defective";      // Send to defects table
    public const string Expired = "Expired";        // Send to expired table
    public const string Damaged = "Damaged";        // Write off
    public const string Used = "Used";           // Cannot return to inventory
}

