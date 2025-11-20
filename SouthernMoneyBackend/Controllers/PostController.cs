using Microsoft.AspNetCore.Mvc;
using Database;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/posts")]
public class PostController : ControllerBase
{
    private readonly PostService postService;
    public PostController(PostService postService)
    {
        this.postService = postService;
    }
    
}
