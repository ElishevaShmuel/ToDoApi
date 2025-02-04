
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.AllowAnyOrigin() 
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseInMemoryDatabase("ToDoList")); 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

 app.UseRouting();

        app.UseCors("AllowAllOrigins"); 

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            _ = endpoints.MapControllers();
        });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();

    app.MapGet("/", () => "Welcome to the ToDo API!");


    app.MapGet("/Items", async (ToDoDbContext db) =>
        await db.Items.ToListAsync());

    app.MapGet("/Items/{Id}", async (int Id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(Id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

    app.MapPost("/Items", async (Item t, ToDoDbContext db) =>
    {
        db.Items.Add(t);
        await db.SaveChangesAsync();
        return Results.Created($"/Items/{t.Id}", t);
    });


    app.MapPut("/Items/{Id}", async (int Id, bool IsComplete, ToDoDbContext db) =>
    {
        var item = await db.Items.FindAsync(Id);
        if (item is null) return Results.NotFound();
        item.IsComplete = IsComplete;
        await db.SaveChangesAsync();
        return Results.NoContent();
    });


    app.MapDelete("/Items/{Id}", async (int Id, ToDoDbContext db) =>
    {
        var item = await db.Items.FindAsync(Id);
        if (item is null) return Results.NotFound();
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.NoContent();
    });
}

app.Run();
