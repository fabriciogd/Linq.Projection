namespace Linq.Projection
{
    using System;
    using System.Linq;

    public interface IProjectionExpression<TSource>
    {
        /// <summary>
        /// Projeta elementos de uma query para uma model compativel
        /// </summary>
        /// <remarks>
        /// Cada propriedade <typeparamref name="TDest"/> é esperada em <typeparamref name="TSource"/> com 
        /// o mesmo nome. A propriedade do <typeparamref name="TSource"/> é atribuida para a propriedade de <typeparamref name="TDest"/>, 
        /// e portanto deve ser do mesmo tipo e nome.
        /// </remarks>
        /// <typeparam name="TDest">Tipo da classe de destino</typeparam>
        /// <returns>Retorna uma query de <typeparamref name="TDest"/></returns>
        IQueryable<TDest> To<TDest>();

        /// <summary>
        /// Projeta elementos de uma query para uma model compativel, com mapeamentos customizados.
        /// </summary>
        /// <remarks>
        /// Cada propriedade <typeparamref name="TDest"/> é esperada em <typeparamref name="TSource"/> com 
        /// o mesmo nome. A propriedade do <typeparamref name="TSource"/> é atribuida para a propriedade de <typeparamref name="TDest"/>, 
        /// e portanto deve ser do mesmo tipo e nome.
        /// <para/>
        /// Para propriedades de destino que não possuem a mesma representação da fonte, use o <param name="customMapper"/> 
        /// para atribuir valor para a propriedade.
        /// </remarks>
        /// <typeparam name="TDest">Tipo da classe de destino</typeparam>
        /// <returns>Retorna uma query de <typeparamref name="TDest"/></returns>
        IQueryable<TDest> To<TDest>(Action<Mapper<TSource, TDest>> customMapper);
    }
}
