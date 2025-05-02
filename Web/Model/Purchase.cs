using System;
using System.Collections.Generic;

namespace Web.Model
{
    public class Purchase
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public List<ItemPurchase> ItemPurchases { get; set; }
    }
}
