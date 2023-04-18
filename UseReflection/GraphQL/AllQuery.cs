using GraphQL;
using System.Threading.Tasks;
using System.Collections.Generic;
using GraphQL.Types;
using System.Linq;
using System;

namespace UseReflection.GraphQL
{
    public class AllQuery
    {
        protected AllQuery() { }

        [GraphQLMetadata("books")]
        public static async Task<List<Book>> QueryBooks(ResolveFieldContext<AllQuery> context) {
            return GetBooks(); 
        }

        [GraphQLMetadata("book")]
        public static async Task<Book> QueryBook(ResolveFieldContext<AllQuery> context, string id) {
            return GetBooks().FirstOrDefault(x => x.Id == Int32.Parse(id));
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
