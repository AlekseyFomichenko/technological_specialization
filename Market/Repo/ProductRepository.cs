using AutoMapper;
using Market.Abstractions;
using Market.Models;
using Market.Models.Dto;

namespace Market.Repo
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMapper _mapper;
        public ProductRepository(IMapper mapper)
        {
            _mapper = mapper;
        }
        public int AddGroup(ProductGroupDto group)
        {
            using (var context = new ProductContext())
            {
                var productGroup = context.ProductGroups.FirstOrDefault(x => x.Name.ToLower() == group.Name.ToLower());
                if (productGroup != null)
                {
                    return productGroup.Id;
                }
                productGroup = _mapper.Map<ProductGroup>(group);
            }
            return group.Id;
        }

        public int AddProduct(Product product)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProductGroupDto> GetGroups()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetProducts()
        {
            throw new NotImplementedException();
        }
    }
}
