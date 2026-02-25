using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Magazine.Core.Models;
using Magazine.Core.Services;

namespace Magazine.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            try
            {
                var products = _productService.GetAll();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetById(Guid id)
        {
            try
            {
                var product = _productService.GetById(id);
                
                if (product == null)
                {
                    return NotFound($"Товар с Id {id} не найден");
                }
                
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpPost]
        public ActionResult<Product> Add([FromBody] Product product)
        {
            try
            {
                var createdProduct = _productService.Add(product);
                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<Product> Remove(Guid id)
        {
            try
            {
                var removedProduct = _productService.Remove(id);
                
                if (removedProduct == null)
                {
                    return NotFound($"Товар с Id {id} не найден");
                }
                
                return Ok(removedProduct);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public ActionResult<Product> Edit(Guid id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest("ID в маршруте не совпадает с ID товара");
                }

                var updatedProduct = _productService.Edit(product);
                return Ok(updatedProduct);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public ActionResult<Product> Search([FromQuery] string term)
        {
            try
            {
                var product = _productService.Search(term);
                
                if (product == null)
                {
                    return NotFound($"Товар по запросу '{term}' не найден");
                }
                
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}