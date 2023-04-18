using GraphQL.Types;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

namespace GraphqlDemo3.GraphQL
{
    public class RootQuery : ObjectGraphType
    {
        public RootQuery()
        {
            //field<listgraphtype<booktype>>("books", resolve:
            //    context => getbooks());
            //Field<BookType>("book", arguments: new QueryArguments(
            //new QueryArgument<IdGraphType> { Name = "id" }
            //), resolve: context =>
            //{
            //    var id = context.GetArgument<int>("id");
            //    return GetBooks().FirstOrDefault(x => x.Id == id);
            //});
            FieldAsync(
                typeof(ListGraphType<BookType>),
                name: "books",
                resolve: async context =>
                {
                    var books = await GetBooksAsync();
                    return books;
                }

            );
            FieldAsync(
                typeof(BookType),
                "book",
                arguments: new QueryArguments(
                   new QueryArgument<IdGraphType> { Name = "id" }
                ),
                resolve: async context => {
                    var id = context.GetArgument<int>("id");
                    var books = await GetBooksAsync();
                    return books.FirstOrDefault(x => x.Id == id);
                }
            );
        }

        static async Task<List<Book>>  GetBooksAsync()
        {
            var books = new List<Book>{
            new Book {
                Id = 1,
                Title = "Fullstack tutorial for GraphQL",
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
            await Task.Delay(1000);
            return books;
        }

        static List<Book> GetBooks()
        {
            var books = new List<Book>{
            new Book {
                Id = 1,
                Title = "Fullstack tutorial for GraphQL",
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
