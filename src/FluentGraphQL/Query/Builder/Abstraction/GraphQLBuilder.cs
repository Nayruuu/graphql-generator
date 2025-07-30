using System.Text;
using System.Collections.Generic;

namespace FluentGraphQL
{
    public abstract class GraphQlBuilder
    {
        protected readonly StringBuilder Builder = new();

        protected readonly Dictionary<string, GraphQLParameter> Parameters = new();
    }
}
