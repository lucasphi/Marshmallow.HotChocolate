using HotChocolate.Execution;
using HotChocolate.Language;
using Marshmallow.HotChocolate.Core.Attributes;
using Marshmallow.HotChocolate.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Marshmallow.Tests")]
namespace Marshmallow.HotChocolate.Core
{
    class GraphToExpressionParser<TEntity>
    {
        private readonly QueryDocument _queryDocument;
        private readonly ExpressionParameters _expressionParameters = new ExpressionParameters();
        private readonly GenericTypeCollection _typeCollection = new GenericTypeCollection();

        private Type _schemaType = typeof(TEntity);

        public GraphToExpressionParser(QueryDocument queryDocument)
        {
            _queryDocument = queryDocument;
        }

        public Expression<Func<TEntity, dynamic>> CreateExpression<TSchema>()
        {
            _schemaType = typeof(TSchema);
            return CreateExpression();
        }

        public Expression<Func<TEntity, dynamic>> CreateExpression()
        {
            var operationDefinition = _queryDocument.Document.Definitions.FirstOrDefault() as OperationDefinitionNode;

            if (operationDefinition.Operation == OperationType.Subscription)
            {
                throw new UnsupportedOperationException(operationDefinition.Operation);
            }

            var fieldNode = operationDefinition.SelectionSet.Selections.FirstOrDefault() as FieldNode;

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
            var joinProperties = new List<GraphScheme>();
            var propertyLookup = new PropertyLookup(type);
            var schemaLookup = new PropertyLookup(_schemaType);
            foreach (FieldNode fieldNode in selections)
            {
                PropertyInfo schemaInfo = schemaLookup.FindProperty(fieldNode.Name.Value);
                if (schemaInfo != null)
                {   
                    var joinAttr = schemaInfo.GetCustomAttribute<JoinAttribute>();
                    if (joinAttr != null)
                    {
                        PropertyInfo propertyInfo = propertyLookup.FindProperty(joinAttr.PropertyName);
                        joinProperties.Add(new GraphScheme(propertyInfo, schemaInfo));
                    }
                    else
                    {
                        PropertyInfo propertyInfo = propertyLookup.FindProperty(fieldNode.Name.Value);
                        GraphExpression graphExpression = CreateGraphExpression(propertyInfo, fieldNode, parameter, parentName);
                        graphExpressions.Add(graphExpression);
                    }
                }
            }

            var joinGroups = joinProperties.GroupBy(f => f.Property.Name);
            foreach (var joinGroup in joinGroups)
            {
                graphExpressions.Add(CreateJoinGraphExpression(parameter, joinGroup));
            }
            return graphExpressions;
        }

        private GraphExpression CreateJoinGraphExpression(ParameterExpression parameter, IGrouping<string, GraphScheme> joinGroup)
        {
            var dynamicProperties = joinGroup.Select(f => new DynamicProperty(f.SchemeProperty.Name, f.SchemeProperty.PropertyType)).ToList();
            var resultType = DynamicClassFactory.CreateType(dynamicProperties, false);

            var bindings = joinGroup.Select(p =>
            {
                var propExp = Expression.PropertyOrField(parameter, p.Property.Name);
                return Expression.Bind(resultType.GetProperty(p.SchemeProperty.Name),
                     Expression.PropertyOrField(propExp, p.SchemeProperty.Name));
            });
            var newExpression = Expression.MemberInit(Expression.New(resultType), bindings);
            return new GraphExpression()
            {
                Property = new DynamicProperty(joinGroup.First().Property.Name, typeof(object)),
                Expression = newExpression
            };
        }

        private GraphExpression CreateGraphExpression(
            PropertyInfo propertyInfo,
            FieldNode fieldNode,
            ParameterExpression parameter,
            string parentName)
        {
            if (propertyInfo.PropertyType.IsGenericCollection())
            {
                return CreateCollectionGraphExpression(fieldNode, parameter, propertyInfo);
            }
            else if (propertyInfo.PropertyType.IsComplex())
            {
                return CreateComplexGraphExpression(fieldNode, parameter, propertyInfo);
            }

            return CreateGraphExpression(parameter, propertyInfo, parentName);
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
    }
}
