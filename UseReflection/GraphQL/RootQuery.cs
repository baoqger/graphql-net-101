using GraphQL.Types;
using System.Collections.Generic;
using System.Linq;

namespace UseReflection.GraphQL
{
    public class RootQuery : ObjectGraphType
    {
        public RootQuery()
        {
            Field<ListGraphType<BookType>>("books", resolve:
                context => GetBooks());
            Field<BookType>("book", arguments: new QueryArguments(
            new QueryArgument<IdGraphType> { Name = "id" }
            ), resolve: context =>
            {
                var id = context.GetArgument<int>("id");
                return GetBooks().FirstOrDefault(x => x.Id == id);
            });
        }

        static List<Book> GetBooks()
        {
            var books = new List<Book>{
            new Book {
                Id = 1,
                Title = "Fullstack tutorial for GraphQL 123",
                Pages = 356
            },
            new Book
            {
            Id = 2,
            Title = "Introductory tutorial to GraphQL",
            Chapters = 10
            },
            new Book
           {
           Id = 3,
           Title = "GraphQL Schema Design for the Enterprise",
           Pages = 550,
           Chapters = 25
           }
       };

            return books;
        }
    }
}
