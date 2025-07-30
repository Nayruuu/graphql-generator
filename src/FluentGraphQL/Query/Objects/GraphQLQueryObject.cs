using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace FluentGraphQL
{
    public abstract class GraphQLQueryObject : GraphQLObject
    {
        public Dictionary<string, GraphQLQueryObjectField> Fields { get; protected set; }
    }

    public class GraphQLQueryObject<T> : GraphQLQueryObject
    {
        private GraphQLQueryObject()
        {
            this.Fields = new Dictionary<string, GraphQLQueryObjectField>();
        }

        public GraphQLQueryObject(string name) : this()
        {
            this.Name = name;
        }

        public GraphQLQueryObject<T> As(string aliasName)
        {
            this.AliasName = aliasName;

            return this;
        }

        public GraphQLQueryObject<T> WithArguments<TArguments>(TArguments arguments) where TArguments : class
        {
            this.Arguments = arguments;

            return this;
        }

        public GraphQLQueryObject<T> Except<TProperty>(
            Expression<Func<T, TProperty>> propertySelector,
            string aliasName = null)
        {
            MemberExpression memberExpression = propertySelector.Body as MemberExpression;
            GraphQLQueryObjectField field = new GraphQLQueryObjectField(memberExpression.Member.Name, aliasName);

            this.Fields.Remove(field.GetPrincipalKey());

            return this;
        }

        public GraphQLQueryObject<T> AddField<TProperty>(
            Expression<Func<T, TProperty>> propertySelector,
            string aliasName = null)
        {
            MemberExpression memberExpression = propertySelector.Body as MemberExpression;
            GraphQLQueryObjectField field = new GraphQLQueryObjectField(memberExpression.Member.Name, aliasName);

            this.Fields[field.GetPrincipalKey()] = field;

            return this;
        }

        public GraphQLQueryObject<T> AddField<TProperty>(
            Expression<Func<T, TProperty>> propertySelector,
            Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
            string aliasName = null) where TProperty : class
        {
            MemberExpression memberExpression = propertySelector.Body as MemberExpression;
            GraphQLQueryObjectField field = complexPropertySelector(new GraphQLQueryObjectField<TProperty>(
                memberExpression.Member.Name, 
                aliasName));

            this.Fields[field.GetPrincipalKey()] = field;

            return this;
        }

        public GraphQLQueryObject<T> AddField<TProperty>(
            Expression<Func<T, IEnumerable<TProperty>>> propertySelector,
            Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
            string aliasName = null) where TProperty : class
        {
            MemberExpression memberExpression = propertySelector.Body as MemberExpression;
            GraphQLQueryObjectField field = complexPropertySelector(new GraphQLQueryObjectField<TProperty>(
                memberExpression.Member.Name, 
                aliasName));

            this.Fields[field.GetPrincipalKey()] = field;

            return this;
        }

        public GraphQLQueryObject<T> AddEveryFields()
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanWrite && property.PropertyType.IsPrimitive());

            foreach (var property in properties)
            {
                this.Fields[property.Name] = new GraphQLQueryObjectField(property.Name, null);
            }

            return this;
        }

        public GraphQLQueryObject<T> AddCollectionField<TProperty>(
            Expression<Func<T, IEnumerable<TProperty>>> propertySelector,
            Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
            string aliasName = null) where TProperty : class
        {
            MemberExpression memberExpression = propertySelector.Body as MemberExpression;
            GraphQLQueryObjectField field = complexPropertySelector(new GraphQLQueryObjectField<TProperty>(
                memberExpression.Member.Name, 
                aliasName));

            this.Fields[field.GetPrincipalKey()] = field;

            return this;
        }
    }
}