using GraphQL.Types;
using GraphQL;

namespace GraphqlDemo3.GraphQL
{
    public class GraphSchema : Schema
    {
        public GraphSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<RootQuery>();
        }
    }
}
