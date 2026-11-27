
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepo;
    private readonly IUserRepository _userRepo;

    public PostsController(
        IPostRepository postRepo,
        IUserRepository userRepo)
    {
        _postRepo = postRepo;
        _userRepo = userRepo;
    }

    // CREATE - POST /Posts
    [HttpPost]
    public async Task<ActionResult<Post>> AddPost(
        [FromBody] Post request)
    {
        Post created = await _postRepo.AddAsync(request);

        return Created($"/Posts/{created.Id}", created);
    }

    // GET SINGLE - GET /Posts/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(int id)
    {
        Post post = await _postRepo.GetSingleAsync(id);

        return Ok(post);
    }

    // GET MANY - GET /Posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts(
        [FromQuery] string? title,
        [FromQuery] int? userId,
        [FromQuery] string? username)
    {
        IQueryable<Post> posts = _postRepo.GetManyAsync();

        if (!string.IsNullOrEmpty(title))
        {
            posts = posts.Where(p => p.Title.Contains(title));
        }

        if (userId.HasValue)
        {
            posts = posts.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(username))
        {
            List<int> userIds = await _userRepo.GetManyAsync()
                .Where(u => u.Username.Contains(username))
                .Select(u => u.Id)
                .ToListAsync();

            posts = posts.Where(p => userIds.Contains(p.UserId));
        }

        return Ok(await posts.ToListAsync());
    }

    // UPDATE - PUT /Posts/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(
        int id,
        [FromBody] Post request)
    {
        Post post = await _postRepo.GetSingleAsync(id);

        post.Title = request.Title;
        post.Body = request.Body;
        post.UserId = request.UserId;
        post.Likes = request.Likes;
        post.Dislikes = request.Dislikes;
        post.Created = request.Created;

        await _postRepo.UpdateAsync(post);

        return NoContent();
    }

    // DELETE - DELETE /Posts/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        await _postRepo.DeleteAsync(id);

        return NoContent();
    }
}
