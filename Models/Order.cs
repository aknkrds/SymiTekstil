namespace symitekstil.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } // 'Pending', 'Approved', 'Completed', vb.
        public User User { get; set; }
        public Product Product { get; set; }
    }
}