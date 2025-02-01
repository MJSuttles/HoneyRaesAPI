namespace HoneyRaesAPI.Models;

public class Employee
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string Speciality { get; set; }
  public List<ServiceTicket> ServiceTickets { get; set; }
}
