namespace Linq.Projection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ProjectionExpression<TSource> : IProjectionExpression<TSource>
    {
        private static readonly Dictionary<long, Expression> _expressionCache = new Dictionary<long, Expression>();

        private readonly IQueryable<TSource> _source;

        public ProjectionExpression(IQueryable<TSource> source)
        {
            this._source = source;
        }

        public IQueryable<TDest> To<TDest>()
        {
            return To<TDest>(mapper => { });
        }

        public IQueryable<TDest> To<TDest>(Action<Mapper<TSource, TDest>> action)
        {
            var _customMappings = new List<Mapping>();
            var _ignoredProperties = new List<PropertyInfo>();

            action(new Mapper<TSource, TDest>(_customMappings, _ignoredProperties));

            var expression = this.BuildExpression<TDest>(_customMappings, _ignoredProperties);

            return this._source.Select(expression);
        }

        private static Expression<Func<TSource, TDest>> GetCachedExpression<TDest>()
        {
            var key = ReflectionHelper.GetHashKey<TSource, TDest>();

            return _expressionCache.ContainsKey(key) ? _expressionCache[key] as Expression<Func<TSource, TDest>> : null;
        }

        private Expression<Func<TSource, TDest>> BuildExpression<TDest>(IEnumerable<Mapping> customMappings, IEnumerable<PropertyInfo> ignoredProperties, Dictionary<int, int> parameterIndexes = null, int index = 0)
        {
            var cachedExp = GetCachedExpression<TDest>();
            if (cachedExp != null)
                return cachedExp;

            if (parameterIndexes == null)
                parameterIndexes = new Dictionary<int, int>();

            var sourceType = typeof(TSource);
            var destinationType = typeof(TDest);

            var sourceProperties = sourceType.GetProperties();
            var destinationProperties = destinationType.GetProperties().Where(dest => dest.CanWrite);

            var parameterExpression = Expression.Parameter(sourceType, string.Concat("src", sourceType.Name));

            var expression =
                Expression.Lambda<Func<TSource, TDest>>(
                    Expression.MemberInit(
                        Expression.New(typeof(TDest)),
                            destinationProperties
                                .Where(dest => !ignoredProperties.Contains(dest))
                                .Select(dest => BindCustomMap(parameterExpression, dest, customMappings) ?? BindSimpleMap(parameterExpression, dest, sourceProperties, parameterIndexes, index))
                                .Where(binding => binding != null)
                                .ToArray()),
                    parameterExpression);

            var key = ReflectionHelper.GetHashKey(sourceType, destinationType);

            if (!_expressionCache.ContainsKey(key))
                _expressionCache.Add(key, expression);

            return expression;

        }

        private MemberAssignment BindCustomMap(ParameterExpression parameterExpression, PropertyInfo destinationProperty, IEnumerable<Mapping> customMappings)
        {
            LambdaExpression customExpression = customMappings.Where(map => map.DestinationPropertyInfo == destinationProperty)
                .Select(map => map.TransformExpression).FirstOrDefault();

            if (customExpression != null)
            {
                var lambda = customExpression as LambdaExpression;

                if (lambda != null)
                {
                    var rightExp = new ParameterRenamer().Rename(customExpression, parameterExpression) as LambdaExpression;
                    return Expression.Bind(destinationProperty, rightExp.Body);
                }

            }

            return null;
        }

        private MemberAssignment BindSimpleMap(Expression parameterExpression, PropertyInfo destinationProperty, IEnumerable<PropertyInfo> sourceProperties, Dictionary<int, int> parameterIndexes, int index, string sourcePropertyName = null)
        {
            var destinationPropertyType = destinationProperty.PropertyType;

            int destinationPropertyTypeHashCode = destinationProperty.GetHashCode();

            if (parameterIndexes.ContainsKey(destinationPropertyTypeHashCode))
                parameterIndexes[destinationPropertyTypeHashCode] = parameterIndexes[destinationPropertyTypeHashCode] + 1;
            else
                parameterIndexes.Add(destinationPropertyTypeHashCode, 1);

            int maxDepth = 3;

            if (parameterIndexes[destinationPropertyTypeHashCode] >= maxDepth)
                return null;

            if (!string.IsNullOrEmpty(sourcePropertyName))
                parameterExpression = Expression.Property(parameterExpression, parameterExpression.Type.GetProperty(sourcePropertyName));

            var sourceProperty = sourceProperties.FirstOrDefault(src => src.Name == destinationProperty.Name);

            if (sourceProperty != null)
            {
                var sourcePropertyType = sourceProperty.PropertyType;

                // String não é um tipo primitivo e sim uma classe
                if (sourcePropertyType.IsClass && sourcePropertyType != typeof(string))
                {
                    var propertiesSourceProperty = sourcePropertyType.GetProperties();

                    var propertiesDestinationProperty = destinationPropertyType.GetProperties().Where(dest => dest.CanWrite);

                    var subMemberExpression = Expression.MemberInit(
                                                Expression.New(destinationPropertyType),
                                                    propertiesDestinationProperty
                                                        .Select(dest => BindSimpleMap(parameterExpression, dest, propertiesSourceProperty, parameterIndexes, index, destinationProperty.Name))
                                                        .Where(binding => binding != null)
                                              );

                    Expression leftExpression = Expression.Property(parameterExpression, sourceProperty);
                    Expression rightExpression = Expression.Constant(null, sourcePropertyType);

                    Expression equalExpression = Expression.Equal(leftExpression, rightExpression);

                    Expression defaultValueExpression = Expression.Constant(null, destinationPropertyType);

                    return Expression.Bind(destinationProperty, Expression.Condition(equalExpression, defaultValueExpression, subMemberExpression));
                }

                else if (sourcePropertyType.IsCollection() && destinationPropertyType.IsCollection())
                {
                    var argumentSourcePropertyType = sourcePropertyType.GetGenericArguments()[0];
                    var argumentDestinationPropertyType = destinationPropertyType.GetGenericArguments()[0];

                    var collectionExpression = typeof(ProjectionExpression<>).MakeGenericType(argumentSourcePropertyType)
                        .GetMethod("BuildExpression", BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(argumentDestinationPropertyType)
                        .Invoke(null, new object[] { null, null, parameterIndexes, ++index });

                    MethodCallExpression selectExpression = Expression.Call(
                        typeof(Enumerable),
                        "Select",
                        new[] { argumentSourcePropertyType, argumentDestinationPropertyType },
                        Expression.Property(parameterExpression, sourceProperty),
                        (Expression)collectionExpression);

                    var convertToListMethod = typeof(Enumerable).GetMethods()
                        .FirstOrDefault(m => m.Name == "ToList")
                        .MakeGenericMethod(argumentDestinationPropertyType);

                    return Expression.Bind(destinationProperty, Expression.Call(convertToListMethod, selectExpression));
                }

                Expression expression = Expression.Property(parameterExpression, sourceProperty);

                if (sourcePropertyType != destinationPropertyType)
                {
                    expression = ReflectionHelper.CreatePrimitiveConvert(expression, destinationPropertyType);
                }

                return Expression.Bind(destinationProperty, expression);
            }

            sourceProperty = sourceProperties.FirstOrDefault(p => p.PropertyType.IsClass &&
                                                                  p.PropertyType != typeof(string) &&
                                                                  destinationProperty.Name.StartsWith(p.Name));

            if (sourceProperty != null)
            {
                var navigationProperty = sourceProperty.PropertyType.GetProperties()
                    .FirstOrDefault(src => src.Name == destinationProperty.Name.Substring(sourceProperty.Name.Length)
                    .TrimStart('_'));

                if (navigationProperty != null)
                {
                    Expression navigationPropertyExpression = Expression.Property(Expression.Property(parameterExpression, sourceProperty), navigationProperty);

                    if (navigationProperty.PropertyType != destinationProperty.PropertyType)
                        navigationPropertyExpression = ReflectionHelper.CreatePrimitiveConvert(navigationPropertyExpression, destinationProperty.PropertyType);

                    Expression leftExpression = Expression.Property(parameterExpression, sourceProperty);
                    Expression rightExpression = Expression.Constant(null, sourceProperty.PropertyType);

                    Expression equalExpression = Expression.Equal(leftExpression, rightExpression);

                    Expression defaultValueExp;

                    if (destinationProperty.PropertyType.IsValueType && !ReflectionHelper.IsNullable(destinationProperty.PropertyType))
                        defaultValueExp = Expression.Constant(Activator.CreateInstance(destinationProperty.PropertyType));
                    else
                        defaultValueExp = Expression.Constant(null, destinationProperty.PropertyType);

                    return Expression.Bind(destinationProperty, Expression.Condition(equalExpression, defaultValueExp, navigationPropertyExpression));
                }
            }

            return null;
        }
    }
}
