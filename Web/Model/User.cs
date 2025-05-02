namespace Web.Model
{
    public class User
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set;}
        public string username { get; set; }
        public string password { get; set; }
        public bool is_admin { get; set; }
        public DateTime date_of_birth { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string sex { get; set; }
        public string address { get; set;}


    }
}
