namespace Online_Shopping_System.Models.Products
{
    public class AddProductRequest
    {
        // Common fields
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }

        // Electronics
        public int? Warranty { get; set; }

        // Books
        public string? Author { get; set; }
        public string? ISBN { get; set; }

        // Clothes
        public int? Size { get; set; }
        public string? Color { get; set; }
    }
}
