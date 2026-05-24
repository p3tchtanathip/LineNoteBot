using LineNoteBot.Data;
using LineNoteBot.Middlewares;
using LineNoteBot.Repositories;
using LineNoteBot.Repositories.Interfaces;
using LineNoteBot.Services;
using LineNoteBot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "LineNoteBot API", Version = "v1" });
    });
}

builder.Services.AddHttpClient<IAiService, AiService>(c =>
{
    c.BaseAddress = new Uri("https://api.groq.com/");
    c.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHttpClient<ILineMessageService, LineMessageService>(c =>
{
    c.BaseAddress = new Uri("https://api.line.me/v2/");
    c.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IAiQueryRateLimiter, AiQueryRateLimiter>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();

var app = builder.Build();

app.Use((context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'none';");

    return next();
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<LineSignatureMiddleware>();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();