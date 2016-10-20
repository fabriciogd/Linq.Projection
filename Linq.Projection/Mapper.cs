namespace Linq.Projection
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Mapeamento customizado
    /// </summary>
    /// <typeparam name="TSource">Tipo da classe da fonte</typeparam>
    /// <typeparam name="TDest">Tipo da classe de destino</typeparam>
    public class Mapper<TSource, TDest>
    {
        private readonly List<Mapping> _customMappings;
        private readonly List<PropertyInfo> _ignoredProperties;

        public Mapper(List<Mapping> customMappings, List<PropertyInfo> ignoredProperties)
        {
            this._customMappings = customMappings;
            this._ignoredProperties = ignoredProperties;
        }

        /// <summary>
        /// Método para mapear a atribuição de valor de uma propriedade de <typeparamref name="TSource"/>
        /// para uma propriedade de <typeparamref name="TDest"/>
        /// </summary>
        /// <typeparam name="TProperty">Tipo da propriedade</typeparam>
        /// <param name="property">Propriedade de <typeparamref name="TDest"/></param>
        /// <param name="transform">Expressão de <typeparamref name="TSource"/></param>
        /// <returns>Retorna a própria classe</returns>
        public Mapper<TSource, TDest> Map<TProperty>(Expression<Func<TDest, TProperty>> property, Expression<Func<TSource, TProperty>> transform)
        {
            this._customMappings.Add(new Mapping(ReflectionHelper.GetProperty(property), transform));

            return this;
        }

        /// <summary>
        /// Método para ignorar o mapeamento de uma propriedade de <typeparamref name="TDest"/>
        /// </summary>
        /// <typeparam name="TProperty">Tipo da propriedade</typeparam>
        /// <param name="property">Propriedade de <typeparamref name="TDest"/></param>
        /// <returns>Retorna a própria classe</returns>
        public Mapper<TSource, TDest> Ignore<TProperty>(Expression<Func<TDest, TProperty>> property)
        {
            this._ignoredProperties.Add(ReflectionHelper.GetProperty(property));

            return this;
        }
    }
}
