using BooksAPI.DTOs;
using BooksAPI.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Collections.Generic;

namespace BooksAPI.Controllers
{
    [RoutePrefix("api/Books")]
    public class BooksController : ApiController
    {
        private BooksAPIContext db = new BooksAPIContext();

        // Typed lambda expression for Select() method. 
        private static readonly Expression<Func<Book, BookDto>> AsBookDto = x => new BookDto
        {
            Title = x.Title,
            Author = x.Author.Name,
            Genre = x.Genre
        };

        [Route("")] //GET api/Books
        [ResponseType(typeof(BookDto))]
        public async Task<IHttpActionResult> GetBooks()
        {
            //SELECIONA AS INFORMÇÕES DO AsBookDto CRIADO ANTERIORMENTE
            //return Ok(await db.Books.Include(a => a.Author).Select(AsBookDto).ToListAsync());

            var book = await (from b in db.Books.Include(b => b.Author)
                              select new BookDto
                              {
                                  Title = b.Title,
                                  Author = b.Author.Name,
                                  Genre = b.Genre
                              }).ToListAsync();

            if (book == null)
            {
                NotFound();
            }
            return Ok(book);
        }

        // GET api/Books/5
        [Route("{id:int}")]
        [ResponseType(typeof(BookDto))]
        public async Task<IHttpActionResult> GetBook(int id)
        {
            BookDto book = await db.Books.Include(b => b.Author)
                .Where(b => b.BookId == id)
                .Select(AsBookDto)
                .FirstOrDefaultAsync();
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [Route("{id:int}/details")]
        [ResponseType(typeof(BookDetailDto))]
        public async Task<IHttpActionResult> GetBookDetail(int id)
        {
            var book = await (from b in db.Books.Include(b => b.Author)
                              where b.BookId == id
                              select new BookDetailDto
                              {
                                  Title = b.Title,
                                  Genre = b.Genre,
                                  PublishDate = b.PublishDate,
                                  Price = b.Price,
                                  Description = b.Description,
                                  Author = b.Author.Name
                              }).FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        [Route("{genre}")]
        [ResponseType(typeof(BookDetailDto))]
        public async Task<IHttpActionResult> GetBooksByGenret(string genre)
        {
            return Ok(await db.Books.Include(a => a.Author)
                        .Where(a => a.Genre.Contains(genre))
                        .Select(AsBookDto).ToListAsync());
        }       

        [HttpGet]
        [Route("~/api/authors/{Author}/books")]
        public async Task<IHttpActionResult> GetBooksByAuthor(string author)
        {

            return Ok(await db.Books.Include(a => a.Author)
                    .Where(a => a.Author.Name.Contains(author))
                    .Select(AsBookDto).ToListAsync());
        }

        [HttpGet]
        [Route("~/api/authors/{AuthorId:int}/books")]
        public async Task<IHttpActionResult> GetBooksByAuthorId(int AuthorId)
        {

            return Ok(await db.Books.Include(a => a.Author)
                    .Where(a => a.Author.AuthorId == AuthorId)
                    .Select(AsBookDto).ToListAsync());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult PostBook(Book book)
        {
            try
            {
                //book = db.Books.Add(book);
                db.Entry(book).State = EntityState.Added;
                db.SaveChanges();

                return Created($"api/Books/{book.BookId}", book);   
            }
            catch (Exception e )
            {
                return BadRequest(e.Message);
            }
           
        }

        [HttpPut]
        [Route("")]
        public IHttpActionResult PutBook(Book book)
        {
            try
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();

                return Ok($"api/Books/{book.BookId}");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);                
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteBook(int id)
        {
            try
            {
                var book = db.Books.Find(id);

                db.Books.Remove(book);
                db.SaveChanges();

                return Ok();
               
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
        

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}