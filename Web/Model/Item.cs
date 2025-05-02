using System;

namespace Web.Model
{
    public class Item
    {
        public int Id { get; set; }
        public string name { get; set; }
        public int stock { get; set; }
        public decimal price { get; set; }
        public string image_path { get; set; }
    }
}
