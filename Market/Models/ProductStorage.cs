namespace Market.Models
{
    public class ProductStorage
    {
        public int? ProductId { get; set; }
        public int? StorageId { get; set; }
        public Product? Product { get; set; }
        public Storage? Storage { get; set; }
    }
}
