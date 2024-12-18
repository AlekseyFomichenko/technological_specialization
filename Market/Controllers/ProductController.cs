using Market.Abstractions;
using Market.Models;
using Market.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;
        public ProductController(IProductRepository productRepository)
        {
            _repository = productRepository;
        }

        [HttpGet("get_products")]
        public IActionResult GetProducts()
        {
            var products = _repository.GetProducts();
            return Ok(products);
        }

        [HttpPut("put_products")]
        public IActionResult AddProducts([FromBody] ProductDto productDto)
        {
            var res = _repository.AddProduct(productDto);
            return Ok(res);
        }

        [HttpGet("get_productGroups")]
        public IActionResult GetProductGroups()
        {
            var groups = _repository.GetGroups();
            return Ok(groups);
        }

        [HttpPut("put_productGroups")]
        public IActionResult AddProductGroups([FromBody] ProductGroupDto productGroupDto)
        {
            var res = _repository.AddGroup(productGroupDto);
            return Ok(res);
        }
    }
}
