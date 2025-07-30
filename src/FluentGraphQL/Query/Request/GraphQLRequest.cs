using System;
using Newtonsoft.Json.Linq;

namespace FluentGraphQL
{
    public class GraphQLRequest
    {
        public string Query { get; set; }

        public JObject Variables { get; set; }

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

        public GraphQLRequest(string query, JObject variables) : this(query)
        {
            this.Variables = variables;
        }
    }
}