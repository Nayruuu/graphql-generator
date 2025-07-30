```

BenchmarkDotNet v0.15.2, macOS Sequoia 15.4.1 (24E263) [Darwin 24.4.0]
Apple M1 Max 2.40GHz, 1 CPU, 10 logical and 10 physical cores
.NET SDK 9.0.301
  [Host]     : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT SSE4.2
  Job-XPUURG : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT SSE4.2

IterationCount=10  WarmupCount=5  

```
| Method              | Mean     | Error    | StdDev   | Gen0   | Allocated |
|-------------------- |---------:|---------:|---------:|-------:|----------:|
| FluentQL            | 12.67 ns | 0.323 ns | 0.192 ns | 0.0020 |      13 B |
| GraphQLQueryBuilder | 12.92 ns | 0.363 ns | 0.240 ns | 0.0018 |      12 B |
