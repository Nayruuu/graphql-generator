using System.Text;
using System.Collections.Generic;

namespace FluentGraphQL
{
    public abstract class GraphQlBuilder
    {
        protected readonly StringBuilder Builder = new StringBuilder();

        protected readonly Dictionary<string, GraphQLParameter> Parameters = new Dictionary<string, GraphQLParameter>();

        protected string GetTabulation(int tabCount)
        {
            return new string('\t', tabCount);
        }
    }
}
