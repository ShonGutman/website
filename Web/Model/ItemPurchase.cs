namespace Web.Model
{
    public class ItemPurchase
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal ItemPriceAtPurchase { get; set; }
        public Item Item { get; set; }
    }
}
