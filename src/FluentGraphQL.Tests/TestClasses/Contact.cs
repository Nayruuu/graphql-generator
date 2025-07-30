using FluentGraphQL.Tests.TestClasses.ValueObjects;

namespace FluentGraphQL.Tests.TestClasses;

public class Contact
{
    public Guid Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public IEnumerable<Task> Tasks { get; set; }
}