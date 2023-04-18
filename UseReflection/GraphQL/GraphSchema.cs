using GraphQL.Types;
using GraphQL;

namespace UseReflection.GraphQL
{
    public class GraphSchema : Schema
    {
        public GraphSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<ObjectGraphTypeHelper<AllQuery>>();
        }
    }
}
