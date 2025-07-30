using FluentGraphQL.Classes.ValueObjects;

namespace FluentGraphQL.Classes;

public class Account
{
    public Guid Id { get; set; }
    
    public string SocietyName { get; set; }
    
    public Adresse Adresse { get; set; }
    
    public IEnumerable<Contact> Contacts { get; set; }
}