using System.Text.RegularExpressions;

using FluentGraphQL.Classes;
using FluentGraphQL.Classes.Inputs;

namespace FluentGraphQL.Tests;

public class Tests
{
    [Fact]
    public void Should_Generate_Mutation()
    {
        var builder = new GraphQLQueryBuilder(mutation: true);
        
        var saveAccountInput = new SaveAccountInput()
        {
            Account = new Account()
            {
                SocietyName = "MyBeautifulSociety",
                Contacts = new List<Contact>()
                {
                    new Contact()
                    {
                        FirstName = "John",
                        LastName = "Paul"
                    }
                }
            }
        };


        builder
            .AddVariable("input", GraphQLParameterType.OBJECT, saveAccountInput)
            .AddQuery(new GraphQLQueryObject<Account>("saveAccount")
                .WithArguments(new { input = "input" })
                .AddField(account => account.Id));
        
        
        Assert.Equal(
            "mutation ($input: SaveAccountInput) { saveAccount(input: $input) { id } }", 
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Account_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddEveryFields());

        Assert.Equal(
            "query { accounts() { id societyName } }", 
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Alias()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .As("myAlias")
                .AddEveryFields());

        Assert.Equal(
            "query { myAlias: accounts() { id societyName } }", 
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Only_Account_Society_Name()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id));
            
        Assert.Equal(
            "query { accounts() { id } }", 
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Contact_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Contact>("contacts")
                .AddEveryFields());

        Assert.Equal(
            "query { contacts() { id firstName lastName email phoneNumber } }",
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Contact_Properties_But_Phone_Number_Excluded()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Contact>("contacts")
                .AddEveryFields()
                .Except(contact => contact.PhoneNumber));

        Assert.Equal(
            "query { contacts() { id firstName lastName email } }",
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Account_And_Adresse_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .AddField(
                    account => account.Adresse,
                    adresse => adresse.AddEveryFields()));

        Assert.Equal(
            "query { accounts() { id societyName adresse { streetNumber zipCode city latitude longitude } } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Account_And_Contacts_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
            .AddCollectionField(
                account => account.Contacts,
                contact => contact.AddEveryFields()));

        Assert.Equal(
            "query { accounts() { id societyName contacts { id firstName lastName email phoneNumber } } }",
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Account_And_Contacts_And_Tasks_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .AddCollectionField(
                    account => account.Contacts,
                    contact => contact
                        .AddEveryFields()
                        .AddCollectionField(
                            c => c.Tasks,
                            task => task.AddEveryFields())));

        Assert.Equal(
            "query { accounts() { id societyName contacts { id firstName lastName email phoneNumber tasks { id name description startDate dueDate } } } }",
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Two_Queries_With_Account_And_Contacts_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields())
            .AddQuery(new GraphQLQueryObject<Contact>("contacts")
                .AddEveryFields());

        Assert.Equal(
            "query { accounts() { id societyName } contacts() { id firstName lastName email phoneNumber } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Query_And_Where_Arguments()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        city = new
                        {
                            eq = "Paris"
                        }
                    }
                }));
        
        Assert.Equal(
            "query { accounts(where: { city: { eq: \"Paris\" } }) { id societyName } }",
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Query_And_Where_In_Array_Arguments()
    {
        var builder = new GraphQLQueryBuilder();
        var cities = new string[] { "Paris", "London", "Madrid", "New York" };

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        city = new
                        {
                            @in = cities
                        }
                    }
                }));
        
        Assert.Equal(
            "query { accounts(where: { city: { in: [ \"Paris\", \"London\", \"Madrid\", \"New York\" ] } }) { id societyName } }",
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Query_And_Variables()
    {
        var builder = new GraphQLQueryBuilder();
        var cities = new string[] { "Paris", "London", "Madrid", "New York" };

        builder
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, cities)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        city = new
                        {
                            @in = "cities"
                        }
                    }
                }));
        
        Assert.Equal(
            "query ($cities: [String]!) { accounts(where: { city: { in: $cities } }) { id societyName } }",
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Query_With_Query_And_Two_Variables()
    {
        var builder = new GraphQLQueryBuilder();
        
        var name = "Paul";
        var cities = new string[] { "Paris", "London", "Madrid", "New York" };

        builder
            .AddVariable("firstName", GraphQLParameterType.STRING, name)
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, cities)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        and = new object[]
                        {
                            new 
                            {
                                city = new
                                {
                                    @in = "cities"
                                }
                            },
                            new
                            {
                                contacts = new
                                {
                                    firstName = new
                                    {
                                        eq = "firstName"
                                    }
                                }
                            }
                        }
                    }
                }));
        
        Assert.Equal(
            "query ($firstName: String!, $cities: [String]!) { accounts(where: { and: [ { city: { in: $cities } }, { contacts: { firstName: { eq: $firstName } } } ] }) { id societyName } }",
            Normalize(builder.Query));
    }
    
    [Fact]
    public void Should_Generate_Variables_With_Query_And_Two_Variables()
    {
        var builder = new GraphQLQueryBuilder();
        
        var name = "Paul";
        var cities = new string[] { "Paris", "London", "Madrid", "New York" };

        builder
            .AddVariable("firstName", GraphQLParameterType.STRING, name)
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, cities)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        and = new object[]
                        {
                            new 
                            {
                                city = new
                                {
                                    @in = "cities"
                                }
                            },
                            new
                            {
                                contacts = new
                                {
                                    firstName = new
                                    {
                                        eq = "firstName"
                                    }
                                }
                            }
                        }
                    }
                }));
        
        Assert.Equal(
            "{ \"firstName\": \"Paul\", \"cities\": [ \"Paris\", \"London\", \"Madrid\", \"New York\" ] }",
            Normalize(builder.Variables.ToString()));
    }
    
    private string Normalize(string query)
    {
        var normalized = Regex
            .Replace(query, @"\n\t*", " ")
            .Trim();

        normalized = Regex
            .Replace(normalized, @"\t+", "")
            .Trim();

        normalized = Regex
            .Replace(normalized, @" {2,}", " ");
        
        return normalized;
    }
}