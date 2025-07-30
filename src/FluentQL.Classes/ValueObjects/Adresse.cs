namespace FluentGraphQL.Classes.ValueObjects;

public class Adresse
{
    public string StreetNumber { get; set; }
    
    public string ZipCode { get; set; }
    
    public string City { get; set; }
    
    public decimal? Latitude { get; set; }
    
    public decimal? Longitude { get; set; }
}