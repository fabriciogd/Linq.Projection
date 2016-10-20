namespace Linq.Projection
{
    using System;
    using System.Collections;
    using System.Data.Objects.SqlClient;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ReflectionHelper
    {
        public static PropertyInfo GetProperty<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            MemberExpression memberExpression;

            if (expression.Body is UnaryExpression)
                memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            else
                memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException("The expression is not a member access expression", "expression");

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("The member access expression does not access a property", "expression");

            var getMethod = property.GetGetMethod(true);
            if (getMethod.IsStatic)
                throw new ArgumentException("The referenced property is a static property", "expression");

            return property;
        }

        public static bool IsCollection(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) && !IsString(type);
        }

        public static bool IsString(this Type type)
        {
            return type == typeof(string);
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Expression CreatePrimitiveConvert(Expression expression, Type destType)
        {
            MethodInfo stringConvertMethodDouble = typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(double?) });
            MethodInfo stringConvertMethodDecimal = typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(decimal?) });

            if (destType == typeof(string))
                return expression;

            var systemType = Nullable.GetUnderlyingType(destType) ?? destType;

            if (systemType == typeof(int)
                || systemType == typeof(long)
                || systemType == typeof(double)
                || systemType == typeof(short)
                || systemType == typeof(byte))
            {
                var doubleExpr = Expression.Convert(expression, typeof(double?));
                return Expression.Call(stringConvertMethodDouble, doubleExpr);
            }

            else if (systemType == typeof(decimal))
            {
                var decimalExpr = Expression.Convert(expression, typeof(decimal?));
                return Expression.Call(stringConvertMethodDecimal, decimalExpr);
            }

            return null;
        }

        public static long GetHashKey<TSource, TDestination>()
        {
            return (typeof(TSource).GetHashCode() << 32) | typeof(TDestination).GetHashCode();
        }

        public static long GetHashKey(Type source, Type destination)
        {
            return (source.GetHashCode() << 32) | destination.GetHashCode();
        }
    }
}
