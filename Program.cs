using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
                     ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();

app.UseCors();

app.UseAuthorization();


// app.UseEndpoints(endpoints =>
// {
//    endpoints.MapControllers();
// });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



//get
app.MapGet("/", () => "Welcome to the ToDo API!");

app.MapGet("/Items", async (ToDoDbContext context1) =>
    await context1.Items.ToListAsync());

app.MapGet("/Items/{Id}", async (int Id, ToDoDbContext context1) =>
{
var item = await context1.Items.FindAsync(Id);
return item is not null ? Results.Ok(item) : Results.NotFound();
});

//post
app.MapPost("/Items", async ([FromBody] Item t, ToDoDbContext context1) =>
{
    context1.Items.Add(t);
    await context1.SaveChangesAsync();
    return Results.Created($"/Items/{t.Id}", t);
});

//put
app.MapPut("/Items/{Id}", async ([FromBody] Item t, int Id, ToDoDbContext context1) =>
{
    var item = await context1.Items.FindAsync(Id);
    if (item is null) return Results.NotFound();
    item.IsComplete = t.IsComplete;
    await context1.SaveChangesAsync();
    return Results.NoContent();
});

//delete
app.MapDelete("/Items/{Id}", async (int Id, ToDoDbContext context1) =>
{
    var item = await context1.Items.FindAsync(Id);
    if (item is null) return Results.NotFound();
    context1.Items.Remove(item);
    await context1.SaveChangesAsync();
    return Results.NoContent();
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
