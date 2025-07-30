using FluentGraphQL.Tests.TestClasses.ValueObjects;

namespace FluentGraphQL.Tests.TestClasses;

public class Account
{
    public Guid Id { get; set; }
    
    public string SocietyName { get; set; }
    
    public Adresse Adresse { get; set; }
    
    public IEnumerable<Contact> Contacts { get; set; }
}