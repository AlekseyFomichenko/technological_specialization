using Market.Models;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        [HttpGet("getProduct")]
        public IActionResult GetProducts()
        {
            try
            {
                using (var context = new ProductContext())
                {
                    var products = context.Products.Select(x => new Product()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description
                    });
                    return Ok(products);
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
        [HttpPut("putProducts")]
        public IActionResult PutProducts([FromQuery] string name, string description, int groupId, int price)
        {
            try
            {
                using (var context = new ProductContext())
                {
                    if (!context.Products.Any(x => x.Name.ToLower().Equals(name)))
                    {
                        context.Add(new Product()
                        {
                            Name = name,
                            Description = description,
                            Price = price,
                            ProductGroupId = groupId
                        });
                        context.SaveChanges();
                        return Ok();
                    }
                    else return StatusCode(404);
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
        [HttpDelete("removeProduct/{productId}")]
        public IActionResult RemoveProduct(int productId)
        {
            try
            {
                using (var context = new ProductContext())
                {
                    var product = context.Products.Find(productId);
                    if (product != null)
                    {
                        context.Products.Remove(product);
                        context.SaveChanges();
                        return Ok();
                    }
                    else return NotFound();
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
        [HttpDelete("removeProductGroup/{productGroupId}")]
        public IActionResult RemoveProductGroup(int productGroupId)
        {
            try
            {
                using (var context = new ProductContext())
                {
                    var group = context.ProductGroups.Find(productGroupId);
                    if (group != null)
                    {
                        var temp = context.Products.Where(x => x.ProductGroupId == productGroupId).ToList();
                        if (temp.Any())
                        {
                            context.Products.RemoveRange(temp);
                        }
                        context.ProductGroups.Remove(group);
                        context.SaveChanges();
                        return Ok();
                    }
                    else return NotFound();
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
        [HttpPut("updateProductPrice/{productPrice}")]
        public IActionResult UpdateProductPrice(int productId, [FromQuery] int newPrice)
        {
            try
            {
                using (var context = new ProductContext())
                {
                    var product = context.Products.Find(productId);
                    if (product != null)
                    {
                        product.Price = newPrice;
                        context.SaveChanges();
                        return Ok();
                    }
                    else return NotFound();
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
