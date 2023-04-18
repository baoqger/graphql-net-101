using GraphQL.Types;

namespace UseReflection.GraphQL
{
    public class BookType : ObjectGraphType<Book>
    {
        public BookType()
        {
            Field(x => x.Id);
            Field(x => x.Title);
            Field(x => x.Pages, nullable: true);
            Field(x => x.Chapters, nullable: true);
        }
    }
}
