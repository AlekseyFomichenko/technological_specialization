using AutoMapper;
using Market.Abstractions;
using Market.Models;
using Market.Models.Dto;
using Microsoft.Extensions.Caching.Memory;

namespace Market.Repo
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        public ProductRepository(IMapper mapper, IMemoryCache cache)
        {
            _mapper = mapper;
            _cache = cache;
        }
        public int AddGroup(ProductGroupDto group)
        {
            using (var context = new ProductContext())
            {
                var productGroup = context.ProductGroups.FirstOrDefault(x => x.Name.ToLower() == group.Name.ToLower());
                if (productGroup == null)
                {
                    productGroup = _mapper.Map<ProductGroup>(group);
                    context.ProductGroups.Add(productGroup);
                    context.SaveChanges();
                    _cache.Remove("groups");
                }
                return productGroup.Id;
            }
        }

        public int AddProduct(ProductDto product)
        {
            using (var context = new ProductContext())
            {
                var newProduct = context.Products.FirstOrDefault(x => x.Name.ToLower() == product.Name.ToLower());
                if (newProduct == null)
                {
                    newProduct = _mapper.Map<Product>(product);
                    context.Products.Add(newProduct);
                    context.SaveChanges();
                    _cache.Remove("products");
                }
                return newProduct.Id;
            }
        }

        public IEnumerable<ProductGroupDto> GetGroups()
        {
            if (_cache.TryGetValue("groups", out List<ProductGroupDto> groups)) return groups;
            using (var context = new ProductContext())
            {
                var list = context.ProductGroups.Select(x => _mapper.Map<ProductGroupDto>(x)).ToList();
                _cache.Set("groups", list, TimeSpan.FromMinutes(30));
                return list;
            }
        }

        public IEnumerable<ProductDto> GetProducts()
        {
            if (_cache.TryGetValue("products", out List<ProductDto> products)) return products;
            using (var context = new ProductContext())
            {
                var list = context.Products.Select(x => _mapper.Map<ProductDto>(x)).ToList();
                _cache.Set("products", list, TimeSpan.FromMinutes(30));
                return list;
            }
        }


    }
}
