using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Language;
using Marshmallow.HotChocolate.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Marshmallow.HotChocolate
{
    class GraphToExpressionParser<TEntity>
    {
        private readonly QueryDocument _queryDocument;
        private readonly ExpressionParameters _expressionParameters = new ExpressionParameters();
        private readonly GenericTypeCollection _typeCollection = new GenericTypeCollection();

        public GraphToExpressionParser(QueryDocument queryDocument)
        {
            _queryDocument = queryDocument;
        }

        public Expression<Func<TEntity, dynamic>> CreateExpression()
        {
            var operation = _queryDocument.Document.Definitions.FirstOrDefault() as OperationDefinitionNode;

            var fieldNode = operation.SelectionSet.Selections.FirstOrDefault() as FieldNode;

            var parameter = Expression.Parameter(typeof(TEntity), _expressionParameters.Next());

            var newExpression = CreateNewExpression(fieldNode, typeof(TEntity), parameter);

            return Expression.Lambda<Func<TEntity, dynamic>>(newExpression, parameter);
        }

        private MemberInitExpression CreateNewExpression(
            FieldNode fieldNode,
            Type type,
            ParameterExpression parameter)
        {
            var selections = fieldNode.SelectionSet.Selections;

            List<GraphExpression> graphExpressions = CreateGraphExpressionList(selections, type, parameter);

            var resultType = DynamicClassFactory.CreateType(graphExpressions.Select(f => f.Property).ToList(), false);
            _typeCollection.AddIfNotExists(type.FullName, resultType);

            var bindings = graphExpressions.Select(p => {
                return Expression.Bind(resultType.GetProperty(p.Property.Name), p.Expression);
            });
            return Expression.MemberInit(Expression.New(resultType), bindings);
        }

        private List<GraphExpression> CreateGraphExpressionList(
            IReadOnlyList<ISelectionNode> selections,
            Type type,
            ParameterExpression parameter,
            string parentName = null)
        {
            var graphExpressions = new List<GraphExpression>();

            var typeProperties = type.GetProperties();
            foreach (var selection in selections)
            {
                GraphExpression graphExpression = CreateGraphExpression(typeProperties, selection, parameter, parentName);
                if (graphExpression != null)
                {
                    graphExpressions.Add(graphExpression);
                }
            }
            return graphExpressions;
        }

        private GraphExpression CreateGraphExpression(
            PropertyInfo[] typeProperties,
            ISelectionNode selection,
            ParameterExpression parameter,
            string parentName)
        {
            var fieldNode = selection as FieldNode;
            var name = fieldNode.Name.Value;
            PropertyInfo prop = FindProperty(typeProperties, name);

            if (prop == null)
            {
                return null;
            }

            var enumerableType = typeof(ICollection<>);
            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition().Equals(enumerableType))
            {
                return CreateCollectionGraphExpression(fieldNode, parameter, prop);
            }
            else if (IsComplex(prop.PropertyType))
            {
                return CreateComplexGraphExpression(fieldNode, parameter, prop); ;
            }

            return CreateGraphExpression(parameter, prop, parentName);
        }

        private static GraphExpression CreateGraphExpression(
            ParameterExpression parameter,
            PropertyInfo prop,
            string parentName)
        {
            Expression expression;
            if (parentName != null)
            {
                expression = Expression.PropertyOrField(parameter, parentName);
            }
            else
            {
                expression = parameter;
            }

            return new GraphExpression
            {
                Property = new DynamicProperty(prop.Name, prop.PropertyType),
                Expression = Expression.PropertyOrField(expression, prop.Name)
            };
        }

        private GraphExpression CreateCollectionGraphExpression(
            FieldNode fieldNode,
            ParameterExpression parameter,
            PropertyInfo prop)
        {
            var genericType = prop.PropertyType.GetGenericArguments().First();
            var innerParameter = Expression.Parameter(genericType, _expressionParameters.Next());

            var innerExpression = CreateNewExpression(fieldNode, genericType, innerParameter);

            var graphExpression = new GraphExpression
            {
                Property = new DynamicProperty(prop.Name, typeof(object)),

                Expression = Expression.Call(typeof(Enumerable),
                                                nameof(Enumerable.Select),
                                                new Type[] { genericType, _typeCollection.Load(genericType.FullName) },
                                                Expression.Property(parameter, prop.Name),
                                                Expression.Lambda(innerExpression, innerParameter))
            };
            return graphExpression;
        }

        private GraphExpression CreateComplexGraphExpression(
            FieldNode fieldNode,
            ParameterExpression parameter,
            PropertyInfo prop)
        {
            List<GraphExpression> graphExpressions = CreateGraphExpressionList(fieldNode.SelectionSet.Selections, prop.PropertyType, parameter, prop.Name);

            var resultType = DynamicClassFactory.CreateType(graphExpressions.Select(f => f.Property).ToList(), false);
            _typeCollection.AddIfNotExists(prop.PropertyType.FullName, resultType);

            var bindings = graphExpressions.Select(p => {
                return Expression.Bind(resultType.GetProperty(p.Property.Name), p.Expression);
            });
            var newExpression = Expression.MemberInit(Expression.New(resultType), bindings);

            return new GraphExpression()
            {
                Property = new DynamicProperty(prop.Name, resultType),
                Expression = newExpression
            };
        }

        private PropertyInfo FindProperty(PropertyInfo[] typeProperties, string name)
        {
            foreach (var prop in typeProperties)
            {
                if (prop.Name.ToLower() == name.ToLower())
                {
                    return prop;
                }

                var nameAttribute = prop.GetCustomAttributes(typeof(GraphQLNameAttribute), false).FirstOrDefault() as GraphQLNameAttribute;
                if (nameAttribute != null && nameAttribute.Name == name)
                {
                    return prop;
                }

            }
            return null;
        }

        private bool IsComplex(Type type)
        {
            return !type.IsPrimitive
                && !type.IsGenericType
                && !type.IsEnum
                && type != typeof(string)
                && type != typeof(DateTime)
                && type != typeof(Guid);
        }
    }
}
