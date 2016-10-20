namespace Linq.Projection
{
    using System.Linq;

    /// <summary>
    /// Método para projetar uma query
    /// </summary>
    /// <typeparam name="TSource">Tipo da classe da fonte</typeparam>
    /// <param name="source">Query</param>
    /// <returns>Retorna <typeparamref name="ProjectionExpression"/></returns>
    public static class Extension
    {
        public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source)
        {
            return new ProjectionExpression<TSource>(source);
        }
    }
}
