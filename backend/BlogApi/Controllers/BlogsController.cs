using BlogApi.Data;
using BlogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly BlogContext _db;

    public BlogsController(BlogContext db)
    {
        _db = db;
    }

    // POST /api/blogs
    [HttpPost]
    public async Task<ActionResult<BlogPost>> Create([FromBody] BlogPost input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        _db.BlogPosts.Add(input);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    // GET /api/blogs
    // Optional: ?page=1&pageSize=10&author=John&from=2024-01-01&to=2025-12-31&search=text
    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? author = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 10;

        var query = _db.BlogPosts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(b => b.Author.Contains(author));

        if (from.HasValue)
            query = query.Where(b => b.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(b => b.CreatedAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Title.Contains(search) || b.Content.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { page, pageSize, total, items });
    }

    // GET /api/blogs/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BlogPost>> GetById([FromRoute] int id)
    {
        var entity = await _db.BlogPosts.FindAsync(id);
        if (entity == null) return NotFound(new { error = "Blog post not found" });
        return Ok(entity);
    }

    // PUT /api/blogs/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BlogPost>> Update([FromRoute] int id, [FromBody] BlogPost input)
    {
        if (id != input.Id)
        {
            // Keep payload and route consistent
            return BadRequest(new { error = "Mismatched id" });
        }
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var entity = await _db.BlogPosts.FindAsync(id);
        if (entity == null) return NotFound(new { error = "Blog post not found" });

        entity.Title = input.Title;
        entity.Content = input.Content;
        entity.Author = input.Author;
        // UpdatedAt handled in SaveChanges

        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    // DELETE /api/blogs/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var entity = await _db.BlogPosts.FindAsync(id);
        if (entity == null) return NotFound(new { error = "Blog post not found" });
        _db.BlogPosts.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
