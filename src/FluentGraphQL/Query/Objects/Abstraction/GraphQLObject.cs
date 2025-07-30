namespace FluentGraphQL
{
    public abstract class GraphQLObject
    {
        public string Name { get; protected set; }

        public string AliasName { get; protected set; }

        public object Arguments { get; protected set; }

        public bool HasAliasName()
        {
            return !string.IsNullOrWhiteSpace(this.AliasName);
        }

        public string GetPrincipalKey()
        {
            return this.HasAliasName() ? this.AliasName : this.Name;
        }
    }
}
