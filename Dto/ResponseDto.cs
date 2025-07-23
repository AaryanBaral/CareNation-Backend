
namespace backend.Dto;
public class SuccessResponseDto<T>
{
    public bool Success { get; } = true;
    public T? Data { get; set; }
}