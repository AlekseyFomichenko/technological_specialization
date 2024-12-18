using Market.Models;
using Market.Models.Dto;

namespace Market.Abstractions
{
    public interface IProductRepository
    {
        public int AddGroup(ProductGroupDto group);
        public IEnumerable<ProductGroupDto> GetGroups();
        public int AddProduct(ProductDto product);
        public IEnumerable<ProductDto> GetProducts();
    }
}
