using System.Transactions;
using backend.Auth;
using backend.Extension;
using backend.Middleware;
using Microsoft.AspNetCore.Authorization;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandlingPath = null; // not using a path
});
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("SensitiveAction",
        p => p.Requirements.Add(new RecentReauthRequirement(TimeSpan.FromMinutes(10))));
});
builder.Services.AddSingleton<IAuthorizationHandler, RecentReauthHandler>();

var app = builder.Build();
app.UseMiddleware<TransactionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAny");
app.MapGet("/", () => "Working");
app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.MapControllers();

app.Run();
