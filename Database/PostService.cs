namespace Database;

public class PostService
{
    private readonly AppDbContext dbContext;
    public PostService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
}