using System.Runtime.CompilerServices;
using HoneyRaesAPI.Models;
List<Customer> customers = new List<Customer>
{
    new Customer() { Id = 100, Name = "Bob Marley", Address = "345 Maple Street" },
    new Customer() { Id = 101, Name = "Sean Connery", Address = "678 Ocean Drive" },
    new Customer() { Id = 102, Name = "Genghis Khan", Address = "1234 Elmwood Avenue" }
};

List<Employee> employees = new List<Employee>
{
    new Employee() { Id = 200, Name = "Steve Perry", Speciality = "Singing" },
    new Employee() { Id = 201, Name = "David Coverdale", Speciality = "Screaming" }
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket() { Id = 1, CustomerId = 100, EmployeeId = 200, Description = "Plugged-up toilet", Emergency = false, DateCompleted = new DateTime(2025, 01, 28) },
    new ServiceTicket() { Id = 2, CustomerId = 101, EmployeeId = 201, Description = "Exposed wires", Emergency = true },
    new ServiceTicket() { Id = 3, CustomerId = 102, Description = "Busted mailbox", Emergency = false },
    new ServiceTicket() { Id = 4, CustomerId = 101, EmployeeId = 200, Description = "Non-functioning monitor", Emergency = false },
    new ServiceTicket() { Id = 5, CustomerId = 100, Description = "Leaky faucet", Emergency = true }
 };

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    return serviceTickets.FirstOrDefault(st => st.Id == id);
});

app.Run();
