using backend.Interface;

namespace backend.Middleware
{
    public class TransactionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                await _next(context);

                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                    await unitOfWork.CommitAsync();
                else
                    await unitOfWork.RollbackAsync();
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }
    }

}