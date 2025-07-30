using System;
using System.Text.Json.Nodes;

namespace FluentGraphQL
{
    public class GraphQLRequest
    {
        public string Query { get; set; }

        public JsonObject Variables { get; set; }

        public GraphQLRequest()
        {
        }

        public GraphQLRequest(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("L'argument 'query' ne peut pas être null");
            }

            this.Query = query;
        }

        public GraphQLRequest(string query, JsonObject variables) : this(query)
        {
            this.Variables = variables;
        }
    }
}