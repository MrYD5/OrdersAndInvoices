namespace TestServer.Data
{
    public class Buyer
    {
        public Buyer()
        {
            this.Orders = new HashSet<Order>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
