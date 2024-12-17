namespace Market.Models
{
    public class ProductGroup : BaseModel
    {
         public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
