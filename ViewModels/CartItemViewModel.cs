public class CartItemViewModel
{
    public int MenuItemId { get; set; }
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; }
    // A calculated property to get the total for this line item.
    public decimal Total => Quantity * UnitPrice;
}