using HotChocolate.Execution;
using HotChocolate.Language;
using Marshmallow.HotChocolate.Attributes;
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

        public GraphToExpressionParser(QueryDocument queryDocument)
        {
            _queryDocument = queryDocument;
        }

        public Expression<Func<TEntity, dynamic>> CreateExpression<TSchema>(bool usePagination)
        {
            if(usePagination)
                return CreateExpression<TSchema>(
                    "edges", 
                    "node", 
                    "nodes", 
                    "pageInfo", 
                    "endCursor", 
                    "hasNextPage", 
                    "hasPreviousPage",
                    "startCursor",
                    "totalCount");
            
            return CreateExpression<TSchema>();
        }

        public Expression<Func<TEntity, dynamic>> CreateExpression<TSchema>(params string[] filters)
        {
            var operationDefinition = _queryDocument.Document.Definitions.FirstOrDefault() as OperationDefinitionNode;

            if (operationDefinition.Operation == OperationType.Subscription)
            {
                throw new UnsupportedOperationException(operationDefinition.Operation);
            }

            var fieldNode = operationDefinition
                .SelectionSet
                .Selections
                .Cast<FieldNode>()
                .FirstOrDefault(s => !filters.Any(f => s.Name.Kind.ToString() == f));

            var parameter = Expression.Parameter(typeof(TEntity), _expressionParameters.Next());

            var newExpression = CreateNewExpression(fieldNode, typeof(TEntity), typeof(TSchema), parameter, filters);

            return Expression.Lambda<Func<TEntity, dynamic>>(newExpression, parameter);
        }

        private MemberInitExpression CreateNewExpression(
            FieldNode fieldNode,
            Type type,
            Type schemaType,
            ParameterExpression parameter,
            params string[] filters)
        {
            var selections = fieldNode.SelectionSet.Selections;

            List<GraphExpression> graphExpressions = CreateGraphExpressionList(selections, type, schemaType, parameter, parentName: null, filters: filters);

            var unambiguousGraphExpressions = new List<GraphExpression>();
            foreach(var expression in graphExpressions)
            {
                if(!unambiguousGraphExpressions.Any(fge => fge.Property.Name == expression.Property.Name))
                    unambiguousGraphExpressions.Add(expression);
            }

            var resultType = DynamicClassFactory.CreateType(unambiguousGraphExpressions.Select(f => f.Property).ToList(), false);
            _typeCollection.AddIfNotExists(type.FullName, resultType);

            var bindings = unambiguousGraphExpressions.Select(p => {
                var property = resultType.GetProperty(p.Property.Name);
                return Expression.Bind(property, p.Expression);
            });

            return Expression.MemberInit(Expression.New(resultType), bindings);
        }

        private List<GraphExpression> CreateGraphExpressionList(
            IReadOnlyList<ISelectionNode> selections,
            Type type,
            Type schemaType,
            Expression parameter,
            string parentName,
            params string[] filters)
        {
            var graphExpressions = new List<GraphExpression>();
            var joinProperties = new List<GraphSchema>();
            var propertyLookup = new PropertyLookup(type);
            var schemaLookup = new PropertyLookup(schemaType);
            foreach (FieldNode fieldNode in selections)
            {
                var currentNodes = FilterNodes(fieldNode, filters);

                foreach(var currentNode in currentNodes)
                {
                    PropertyInfo schemaInfo = schemaLookup.FindProperty(currentNode.Name.Value);
                    if (schemaInfo != null)
                    {   
                        var joinAttr = schemaInfo.GetCustomAttribute<JoinAttribute>();
                        if (joinAttr != null)
                        {
                            PropertyInfo propertyInfo = propertyLookup.FindProperty(joinAttr.PropertyName);
                            joinProperties.Add(new GraphSchema(propertyInfo, schemaInfo, fieldNode));
                        }
                        else
                        {
                            PropertyInfo propertyInfo = FindPropertyInfo(propertyLookup, currentNode.Name.Value, schemaInfo);
                            GraphExpression graphExpression = CreateGraphExpression(propertyInfo, currentNode, parameter, schemaType, parentName);
                            graphExpressions.Add(graphExpression);
                        }
                    }
                }
            }

            var graphSchemaGroups = joinProperties.GroupBy(f => f.Property.Name);
            foreach (var schemaGroup in graphSchemaGroups)
            {
                graphExpressions.Add(CreateJoinGraphExpression(parameter, schemaGroup));
            }

            return graphExpressions;
        }

        private IEnumerable<FieldNode> FilterNodes(FieldNode fieldNode, params string[] filters)
        {
            IEnumerable<FieldNode> currentNodes = new List<FieldNode>{ fieldNode };

            while(currentNodes.Any(n => filters.Any(f => f == n.Name.Value)))
            {
                currentNodes = currentNodes
                    .Where(n => filters.Any(f => f == n.Name.Value))
                    .SelectMany(n => n?.SelectionSet?.Selections ?? new List<FieldNode>())
                    .Cast<FieldNode>();
            }

            return currentNodes;
        }

        private GraphExpression CreateJoinGraphExpression(Expression parameter, IEnumerable<GraphSchema> schemaGroup)
        {
            Type resultType = CreateJoinType(schemaGroup);

            var bindings = schemaGroup.Select(schema =>
            {
                var parentPropertyExpression = Expression.PropertyOrField(parameter, schema.Property.Name);
                Expression expression;
                if (schema.SchemaProperty.PropertyType.IsGenericCollection())
                {
                    var graph = CreateGraphExpression(schema.SchemaProperty, schema.FieldNode, parentPropertyExpression, schema.Property.PropertyType, null);
                    expression = graph.Expression;
                }
                else
                {
                    expression = Expression.PropertyOrField(parentPropertyExpression, schema.FoundPropertyName);
                }
                return Expression.Bind(resultType.GetProperty(schema.FoundPropertyName), expression);
            });

            var newExpression = Expression.MemberInit(Expression.New(resultType), bindings);
            return new GraphExpression()
            {
                Property = new DynamicProperty(schemaGroup.First().Property.Name, typeof(object)),
                Expression = newExpression
            };
        }

        private Type CreateJoinType(IEnumerable<GraphSchema> schemaGroup)
        {
            var propertyLookup = new PropertyLookup(schemaGroup.First().Property.PropertyType);
            var dynamicProperties = schemaGroup.Select(groupItem =>
            {
                var property = FindPropertyInfo(propertyLookup, groupItem.SchemaProperty.Name, groupItem.SchemaProperty);
                var propType = property.PropertyType;
                groupItem.FoundPropertyName = property.Name;
                if (!propType.IsTypePrimitive())
                {
                    propType = typeof(object);
                }
                return new DynamicProperty(property.Name, propType);
            }).ToList();
            var resultType = DynamicClassFactory.CreateType(dynamicProperties, false);
            return resultType;
        }

        private PropertyInfo FindPropertyInfo(PropertyLookup propertyLookup, string propertyName, PropertyInfo schemaInfo)
        {
            var propertyInfo = propertyLookup.FindProperty(propertyName, schemaInfo);
            if (propertyInfo == null)
            {
                throw new PropertyNotFoundException(propertyLookup.Type.Name, propertyName);
            }
            return propertyInfo;
        }

        private GraphExpression CreateGraphExpression(
            PropertyInfo propertyInfo,
            FieldNode fieldNode,
            Expression parameter,
            Type schemaType,
            string parentName)
        {
            Expression expression = (parentName != null) ? Expression.PropertyOrField(parameter, parentName) : parameter;
            
            if (propertyInfo.PropertyType.IsGenericCollection())
            {
                return CreateCollectionGraphExpression(fieldNode, expression, propertyInfo, schemaType);
            }
            else if (!propertyInfo.PropertyType.IsTypePrimitive())
            {
                return CreateComplexGraphExpression(fieldNode, expression, propertyInfo, schemaType);
            }

            return new GraphExpression
            {
                Property = new DynamicProperty(propertyInfo.Name, propertyInfo.PropertyType),
                Expression = Expression.PropertyOrField(expression, propertyInfo.Name)
            };
        }

        private GraphExpression CreateCollectionGraphExpression(
            FieldNode fieldNode,
            Expression parameter,
            PropertyInfo prop,
            Type schemaType)
        {
            var genericType = prop.PropertyType.GetGenericArguments().First();
            var innerParameter = Expression.Parameter(genericType, _expressionParameters.Next());
            var childSchemaType = schemaType.GetProperty(prop.Name).PropertyType.GetGenericArguments().First();

            var innerExpression = CreateNewExpression(fieldNode, genericType, childSchemaType, innerParameter);

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
            Expression parameter,
            PropertyInfo prop,
            Type schemaType)
        {
            var childSchemaType = schemaType.GetProperty(prop.Name).PropertyType;

            List<GraphExpression> graphExpressions = CreateGraphExpressionList(
                fieldNode.SelectionSet.Selections,
                prop.PropertyType,
                childSchemaType,
                parameter,
                prop.Name);

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
