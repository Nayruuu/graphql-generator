using BenchmarkDotNet.Attributes;
using FluentGraphQL;
using FluentGraphQL.Classes;
using GraphQL.Query.Builder;

namespace FluentQL.Benchmark.Benchmarks;

[MemoryDiagnoser]
[WarmupCount(5)]
[IterationCount(10)]
public class GraphQLBuilderBenchmark
{
    [Benchmark(OperationsPerInvoke = 1000)]
    public string FluentQL()
    {
        var builder = new GraphQLQueryBuilder();
        
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(x => x.Id)
            .AddField(x => x.SocietyName)
            .AddCollectionField(
                x => x.Contacts,
                contact => contact
                    .AddField(c => c.Id)
                    .AddField(c => c.FirstName)
                    .AddField(c => c.LastName)
                    .AddField(c => c.Email)
                    .AddField(c => c.PhoneNumber)
                    .AddCollectionField(
                        c => c.Tasks,
                        task => task
                            .AddField(t => t.Id)
                            .AddField(t => t.Description)
                            .AddField(t => t.DueDate)
                            .AddField(t => t.StartDate)
                            .AddField(t => t.Name))));
        
        var query = builder.Query;

        return query;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public string GraphQLQueryBuilder()
    {
        var builder = new Query<Account>("accounts");

        builder
            .AddField(x => x.Id)
            .AddField(x => x.SocietyName)
            .AddField(x => x.Contacts, contact => contact
                .AddField(c => c.Id)
                .AddField(c => c.FirstName)
                .AddField(c => c.LastName)
                .AddField(c => c.Email)
                .AddField(c => c.PhoneNumber)
                .AddField(c => c.Tasks, task => task
                    .AddField(t => t.Id)
                    .AddField(t => t.Description)
                    .AddField(t => t.DueDate)
                    .AddField(t => t.StartDate)
                    .AddField(t => t.Name)));

        var query = builder.Build();

        return query;
    }
}