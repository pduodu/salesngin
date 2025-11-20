namespace salesngin.Models;

public class CartUpdateResult
{
    public List<CartItem> CartItems { get; set; } = [];
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}