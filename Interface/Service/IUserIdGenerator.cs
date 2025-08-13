
public interface IUserIdGenerator
{
    Task<string> NextAsync();
}
