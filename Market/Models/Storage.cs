namespace Market.Models
{
    public class Storage : BaseModel
    {
        public virtual ICollection<Product>? Products { get; set; }
        public int Count { get; set; }
    }
}