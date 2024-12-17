namespace Market.Models
{
    public class Product : BaseModel
    {
        public double? Price { get; set; }
        public int ProductGroupId { get; set; }
        public virtual ProductGroup? ProductGroup { get; set; }
        public virtual ICollection<Storage>? Storages {  get; set; } 
    }
}
