using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        
        public JsonObject Variables
        {
            get
            {
                var jsonObject = new JsonObject();

                foreach (var param in Parameters)
                {
                    if (param.Value?.Value is not null)
                    {
                        jsonObject[param.Key.ToCamelCase()] = JsonValue.Create(param.Value.Value);
                    }
                }

                return jsonObject;
            }
        }
        
        public int QueriesCount => queries.Count;

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

                        return $"${parameter.Key}: {type}";
                    });

                this.Builder.Append(string.Join(", ", graphQLParameters));
                this.Builder.Append(")");
            }

            this.Builder.AppendLine(" {");

            foreach (var query in queries)
            {
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

                this.Builder.AppendLine("}");
            }

            this.Builder.AppendLine("}");
        }

        private void AppendArguments(object arguments, bool fromObject = false)
        {
            var properties = arguments.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            bool first = true;

            foreach (var property in properties)
            {
                var key = property.Name;
                var value = property.GetValue(arguments);

                if (first) first = false;
                else this.Builder.Append(", ");

                this.Builder.Append(key.ToCamelCase());
                this.Builder.Append(": ");

                if (value is null)
                {
                    this.Builder.Append("null");
                }
                else if (value is Array array)
                {
                    this.Builder.Append("[ ");
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i > 0) this.Builder.Append(", ");
                        var item = array.GetValue(i);

                        if (item is null)
                        {
                            this.Builder.Append("null");
                        }
                        else if (item.GetType().IsClass && item is not string)
                        {
                            this.Builder.Append("{ ");
                            AppendArguments(item, true);
                            this.Builder.Append(" }");
                        }
                        else
                        {
                            this.Builder.Append(FormatQueryArgument(item));
                        }
                    }
                    this.Builder.Append(" ]");
                }
                else if (value.GetType().IsClass && value is not string)
                {
                    this.Builder.Append("{ ");
                    AppendArguments(value, true);
                    this.Builder.Append(" }");
                }
                else
                {
                    this.Builder.Append(FormatQueryArgument(value));
                }
            }
        }

        private void AppendFields(Dictionary<string, GraphQLQueryObjectField> fields, int tabCount)
        {
            foreach (var field in fields)
            {
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