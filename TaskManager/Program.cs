using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.API.Hubs;
using TaskManager.Data.Context;
using TaskManager.Data.Interfaces;
using TaskManager.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<TaskManagerContext>(options =>
                                                    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagerConnString")));
builder.Services.AddSignalR();

builder.Services.AddTransient<ITask, TaskRepository>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseRouting();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<TaskHub>("/taskHub");

app.Run();
