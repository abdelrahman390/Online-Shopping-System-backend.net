namespace Online_Shopping_System.Models.Products
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        // Types: Electronics - Clothes - Books
        public string Type { get; set; }

    }
}
