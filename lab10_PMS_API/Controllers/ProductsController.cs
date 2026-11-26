using lab10_PMS_API.Data;
using lab10_PMS_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProductsController(
        AppDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: api/products
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found");

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromForm] Product product,
        IFormFile? photo)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (photo == null || photo.Length == 0)
            return BadRequest("Product image is required");

        string? imageError = ValidateImage(photo);

        if (imageError != null)
            return BadRequest(imageError);

        product.Image = await SaveImage(photo);

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product
        );
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromForm] Product updatedProduct,
        IFormFile? photo)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (photo != null)
        {
            string? imageError = ValidateImage(photo);

            if (imageError != null)
                return BadRequest(imageError);

            string oldImage = product.Image;

            product.Image = await SaveImage(photo);

            DeleteImage(oldImage);
        }

        product.Title = updatedProduct.Title;
        product.Description = updatedProduct.Description;
        product.Price = updatedProduct.Price;
        product.Quantity = updatedProduct.Quantity;
        product.Status = updatedProduct.Status;

        await _context.SaveChangesAsync();

        return Ok(product);
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found");

        DeleteImage(product.Image);

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return Ok("Product deleted successfully");
    }

    // Image validation
    private string? ValidateImage(IFormFile photo)
    {
        var allowedExtensions = new[]
        {
                ".jpg", ".jpeg", ".png", ".webp"
            };

        string extension =
            Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return "Only JPG, JPEG, PNG and WEBP images are allowed.";

        if (photo.Length > 5 * 1024 * 1024)
            return "Maximum image size is 5 MB.";

        if (photo.Length == 0)
            return "Empty image is not allowed.";

        return null;
    }

    // Save image
    private async Task<string> SaveImage(IFormFile photo)
    {
        string folder = Path.Combine(
            _environment.WebRootPath ??
            Path.Combine(_environment.ContentRootPath, "wwwroot"),
            "uploads"
        );

        Directory.CreateDirectory(folder);

        string fileName = Guid.NewGuid().ToString()
            + Path.GetExtension(photo.FileName).ToLowerInvariant();

        string filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }

        return "/uploads/" + fileName;
    }

    // Delete image
    private void DeleteImage(string image)
    {
        if (string.IsNullOrWhiteSpace(image))
            return;

        string fileName = Path.GetFileName(image);

        string filePath = Path.Combine(
            _environment.WebRootPath ??
            Path.Combine(_environment.ContentRootPath, "wwwroot"),
            "uploads",
            fileName
        );

        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }
}
