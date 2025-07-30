using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace FluentGraphQL
{
    public class GraphQLQueryBuilder : GraphQlBuilder
{
    private readonly bool mutation;
    private readonly Dictionary<string, GraphQLQueryObject> queries;

    public string Query
    {
        get
        {
            this.BuildQuery();

            return this.Builder.ToString();
        }
    }

    public int QueriesCount => queries.Count;

    public JObject Variables
    {
        get
        {
            return JObject.FromObject(
                Parameters.ToDictionary(x => x.Key, x => x.Value.Value),
                JsonSerializer.Create(new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                })).CleanObject();
        }
    }

    public GraphQLRequest Request => new GraphQLRequest(Query, Variables);

    public GraphQLQueryBuilder(bool mutation = false) : base()
    {
        this.mutation = mutation;
        this.queries = new Dictionary<string, GraphQLQueryObject>();
    }

    public GraphQLQueryBuilder AddVariable(GraphQLParameter parameter)
    {
        this.Parameters[parameter.Name] = parameter;

        return this;
    }

    public GraphQLQueryBuilder AddVariables(params GraphQLParameter[] parameters)
    {
        foreach (var parameter in parameters)
        {
            this.AddVariable(parameter);
        }

        return this;
    }

    public GraphQLQueryBuilder AddVariable(string name, GraphQLParameterType type, object value)
    {
        this.AddVariable(new GraphQLParameter() { Name = name, Type = type, Value = value });

        return this;
    }

    public GraphQLQueryBuilder AddScalarQuery<T>(GraphQLQueryObject<T> queryObject) where T : struct
    {
        var queryName = queryObject.HasAliasName() ? queryObject.AliasName : queryObject.Name;

        this.queries[queryName] = queryObject;

        return this;
    }

    public GraphQLQueryBuilder AddQuery<T>(GraphQLQueryObject<T> queryObject) where T : class
    {
        var queryName = queryObject.HasAliasName() ? queryObject.AliasName : queryObject.Name;

        this.queries[queryName] = queryObject;

        return this;
    }

    private void BuildQuery()
    {
        int tabCount = 1;

        this.Builder.Clear();

        this.Builder.AppendLine(mutation ? "mutation" : "query");

        if (Parameters.Any())
        {
            this.Builder.Append(" (");
            var graphQLParameters = Parameters
                .Where(parameter => parameter.Value.Value != null)
                .Select(parameter =>
                {
                    string type = parameter.Value.Type switch
                    {
                        GraphQLParameterType.INT => "Int!",
                        GraphQLParameterType.STRING => "String!",
                        GraphQLParameterType.DATETIME => "DateTime!",
                        GraphQLParameterType.BOOLEAN => "Boolean!",
                        GraphQLParameterType.STRING_ARRAY => "[String]!",
                        GraphQLParameterType.INT_ARRAY => "[Int!]!",
                        GraphQLParameterType.DATETIME_ARRAY => "[DateTime]!",
                        GraphQLParameterType.OBJECT => parameter.Value.Value.GetType().Name,
                        GraphQLParameterType.UUID => "UUID!",
                        _ => throw new NotImplementedException()
                    };

                    return GetTabulation(tabCount) + $"${parameter.Key}: {type}";
                });

            this.Builder.Append(string.Join("," + Environment.NewLine, graphQLParameters));
            this.Builder.Append(")");
        }

        this.Builder.AppendLine(" {");

        foreach (var query in queries)
        {
            this.Builder.Append(GetTabulation(tabCount));

            if (query.Value.HasAliasName())
            {
                this.Builder.Append($"{query.Value.AliasName}: ");
            }

            this.Builder.Append($"{query.Value.Name}(");

            if (!(query.Value.Arguments is null))
            {
                this.AppendArguments(query.Value.Arguments);
            }

            this.Builder.AppendLine(") {");

            this.AppendFields(query.Value.Fields, tabCount + 1);

            this.Builder.Append(GetTabulation(tabCount));

            this.Builder.AppendLine("}");
        }

        this.Builder.AppendLine("}");
    }

    private void AppendArguments(object arguments, bool fromObject = false)
    {
        var properties = arguments
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            string key = property.Name;
            object value = property.GetValue(arguments, null);

            if (value is null)
            {
                this.Builder.Append($"{key}: null");
            }
            else if (value.GetType().IsArray)
            {
                this.Builder.Append($"{key}: ");

                if (key.Equals("OR", StringComparison.InvariantCultureIgnoreCase) ||
                    key.Equals("AND", StringComparison.InvariantCultureIgnoreCase))
                {
                    fromObject = false;
                }

                var array = value as Array;
                this.Builder.Append("[ ");
                for (int i = 0; i < array.Length; i++)
                {
                    var item = array.GetValue(i);

                    if (fromObject == false)
                    {
                        this.Builder.Append("{ ");
                        this.AppendArguments(item);
                        this.Builder.Append(i == (array.Length - 1) ? " }" : " },");
                    }
                    else
                    {
                        string strValue = this.FormatQueryArgument(item);

                        this.Builder.Append(strValue);
                        this.Builder.Append(i == (array.Length - 1) ? "" : ", ");
                    }
                }

                this.Builder.Append(" ]");
            }
            else if (value.GetType() != typeof(string) && value.GetType().IsClass)
            {
                if (key.Equals("OR", StringComparison.InvariantCultureIgnoreCase) ||
                    key.Equals("AND", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Builder.Append($"{key.ToLower()}: ");

                    this.Builder.Append("[ ");

                    this.AppendArguments(value, false);

                    this.Builder.Append(" ]");
                }
                else
                {
                    this.Builder.Append($"{key.ToCamelCase()}: ");

                    this.Builder.Append("{ ");

                    this.AppendArguments(value, true);

                    this.Builder.Append(" }");
                }
            }
            else
            {
                string argument = FormatQueryArgument(value);

                if (argument != null)
                {
                    this.Builder.Append($"{key.ToCamelCase()}: ");
                    this.Builder.Append(argument);
                }
            }

            this.Builder.Append(", ");
        }

        if (properties.Any())
        {
            this.Builder.Length -= 2;
        }
    }

    private void AppendFields(Dictionary<string, GraphQLQueryObjectField> fields, int tabCount)
    {
        foreach (var field in fields)
        {
            this.Builder.Append(GetTabulation(tabCount));

            if (field.Value.HasAliasName())
            {
                this.Builder.Append($"{field.Value.AliasName}: ");
            }

            if (field.Value.Fields.Count > 0)
            {
                if (!(field.Value.Arguments is null))
                {
                    this.Builder.Append($"{field.Value.Name.ToCamelCase()} (");
                    this.AppendArguments(field.Value.Arguments);
                    this.Builder.AppendLine(") {");
                }
                else
                {
                    this.Builder.AppendLine($"{field.Value.Name.ToCamelCase()} {{");
                }


                this.AppendFields(field.Value.Fields, tabCount + 1);

                this.Builder.Append(GetTabulation(tabCount));
                this.Builder.AppendLine("}");
            }
            else
            {
                this.Builder.AppendLine($"{field.Value.Name.ToCamelCase()}");
            }
        }
    }

    private string FormatQueryArgument(object value)
    {
        string FormatEnumerableValue(IEnumerable value)
        {
            var items = new List<string>();

            foreach (var item in value)
            {
                string argument = FormatQueryArgument(item);

                if (argument != null)
                {
                    items.Add(argument);
                }
            }

            return $"[{string.Join(",", items)}]";
        }

        ;

        if (value is string && Parameters.ContainsKey(value as string))
        {
            if (Parameters[value as string].Value != null)
            {
                return $"${value}";
            }

            return null;
        }

        return value switch
        {
            bool booleanValue => value.ToString().ToLower(),
            string strValue => "\"" + strValue + "\"",
            float floatValue => floatValue.ToString(CultureInfo.CreateSpecificCulture("en-us")),
            double doubleValue => doubleValue.ToString(CultureInfo.CreateSpecificCulture("en-us")),
            decimal decimalValue => decimalValue.ToString(CultureInfo.CreateSpecificCulture("en-us")),
            IEnumerable enumerableValue => FormatEnumerableValue(enumerableValue),
            _ => value.ToString()
        };
    }
}
}

