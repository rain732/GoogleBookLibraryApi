using GoogleBookLibraryApi.Models;
using GoogleBookLibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<GoogleBooksService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<GoogleBooksService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/books/search", async (
    [FromQuery(Name = "q")] string? query,
    [FromServices] GoogleBooksService service) =>
{
    if (string.IsNullOrWhiteSpace(query))
        return Results.BadRequest(new { message = "Missing search query parameter '?q='" });

    try
    {
        var results = await service.SearchBooksAsync(query);
        if (results.Success && results.ErrorMessage == null)
            return Results.Ok(results.Books);
        else
            return Results.BadRequest(new { results.ErrorMessage });        
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to fetch books: {ex.Message}");
    }
});

app.Run();
