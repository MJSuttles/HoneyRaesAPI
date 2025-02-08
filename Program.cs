using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using HoneyRaesAPI.Models;
using Microsoft.AspNetCore.Authentication;

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

// Fix for cycle error
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ENDPOINTS

// Service Tickets

// GET all service tickets
app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

// GET service tickets by Id
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapGet("/servicetickets/completed", () =>
{
    List<ServiceTicket> completedTickets = serviceTickets
                        .Where(st => st.DateCompleted != null)
                        .OrderBy(st => st.DateCompleted)
                        .ToList();

    if (!completedTickets.Any())
    {
        return Results.NotFound();
    }

    return Results.Ok(completedTickets);
});

// Employees

// GET all employees
app.MapGet("/employees", () =>
{
    return employees;
});

// GET employees by Id 
app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

// GET available employees
app.MapGet("/employees/available", () =>
{
    List<int> assignedEmployeeIds = serviceTickets
        .Where(st => st.DateCompleted == null && st.EmployeeId.HasValue)
        .Select(st => st.EmployeeId.Value)
        .Distinct()
        .ToList();

    List<Employee> unassignedEmployees = employees
        .Where(e => !assignedEmployeeIds.Contains(e.Id))
        .ToList();

    return unassignedEmployees;
});

// GET employees' customers
app.MapGet("/employees/{id}/customers", (int id) =>
{
    List<int> customerIds = serviceTickets
        .Where(st => st.EmployeeId == id)
        .Select(st => st.CustomerId)
        .Distinct()
        .ToList();

    List<Customer> employeeCustomers = customers
        .Where(c => customerIds.Contains(c.Id))
        .ToList();

    return employeeCustomers;
});

// GET employee of the month
app.MapGet("/employees/employee-of-the-month", () =>
{
    DateTime today = DateTime.Today;
    DateTime firstDayOfLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
    DateTime lastDayOfLastMonth = firstDayOfLastMonth.AddMonths(1).AddDays(-1);

    List<ServiceTicket> lastMonthTickets = serviceTickets
        .Where(st => st.DateCompleted != null &&
               st.DateCompleted >= firstDayOfLastMonth &&
               st.DateCompleted <= lastDayOfLastMonth)
               .ToList();

    if (!lastMonthTickets.Any())
    {
        return Results.NotFound();
    }

    int? topEmployeeId = lastMonthTickets
                   .GroupBy(st => st.EmployeeId)
                   .OrderByDescending(g => g.Count())
                   .Select(g => g.Key)
                   .FirstOrDefault();

    if (topEmployeeId == null)
    {
        return Results.NotFound();
    }

    Employee topEmployee = employees.FirstOrDefault(e => e.Id == topEmployeeId.Value);

    if (topEmployee == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(topEmployee);
});

// Customers

// GET all customers
app.MapGet("/customers", () =>
{
    return customers;
});

// Get customers by Id
app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

// GET inactive customers
app.MapGet("/customers/inactive", () =>
{
    DateTime oneYearAgo = DateTime.Today.AddYears(-1);

    List<Customer> inactiveCustomers = customers.Where(c => !serviceTickets.Any(st => st.CustomerId == c.Id && st.DateCompleted != null && st.DateCompleted >= oneYearAgo)).ToList();
    if (!inactiveCustomers.Any())
    {
        return Results.NotFound();
    }
    return Results.Ok(inactiveCustomers);
});

// Service Tickets

// POST new service ticket
app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

// Delete Service Ticket
app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTickets.Remove(serviceTicket);
    return Results.Ok();
});

// PUT service ticket
app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

// GET emergency service tickets
app.MapGet("/servicetickets/emergencies", () =>
{
    List<ServiceTicket> emergencyTickets = serviceTickets.Where(st => st.DateCompleted == null && st.Emergency == true).ToList();
    if (!emergencyTickets.Any())
    {
        return Results.NotFound();
    }
    return Results.Ok(emergencyTickets);
});

// GET unassigned service tickets
app.MapGet("/servicetickets/unassigned", () =>
{
    List<ServiceTicket> unassignedTickets = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    if (!unassignedTickets.Any())
    {
        return Results.NotFound();
    }
    return Results.Ok(unassignedTickets);
});

// GET prioritized tickets
app.MapGet("/servicetickets/prioritized", () =>
{
    List<ServiceTicket> orderedIncompleteTickets = serviceTickets
                        .Where(st => st.DateCompleted == null)
                        .OrderByDescending(st => st.Emergency)
                        .ThenBy(st => st.EmployeeId)
                        .ToList();

    if (!orderedIncompleteTickets.Any())
    {
        return Results.NotFound();
    }

    return Results.Ok(orderedIncompleteTickets);
});

app.Run();
