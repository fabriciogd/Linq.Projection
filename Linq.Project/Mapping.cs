namespace Linq.Projection
{
    using System.Linq.Expressions;
    using System.Reflection;

    public class Mapping
    {
        public PropertyInfo DestinationPropertyInfo { get; private set; }
        public LambdaExpression TransformExpression { get; private set; }

        public Mapping(PropertyInfo destinationPropertyInfo, LambdaExpression transformExpression)
        {
            this.DestinationPropertyInfo = destinationPropertyInfo;
            this.TransformExpression = transformExpression;
        }
    }
}
