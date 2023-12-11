using System.Net;
using AutoMapper;
using Entities;
using Filters;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;
    private readonly IMapper _mapper;

    public BooksController(IBooksRepository booksRepository, IMapper mapper)
    {
        _booksRepository = booksRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [TypeFilter(typeof(BookResultFilter))]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllAsync()
    {
        var books = await _booksRepository.GetAllAsync();
        return Ok(books);
    }

    [HttpGet("{id}", Name = "GetBook")]
    [TypeFilter(typeof(BookResultFilter))]
    public async Task<ActionResult<BookDto>> GetAsync(int id)
    {
        var book = await _booksRepository.GetAsync(id);
        if (book is null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpPost]
    [TypeFilter(typeof(BookResultFilter))]
    public async Task<ActionResult<Book>> CreateAsync([FromBody] CreateBookDto createBook)
    {
        var bookEntity = _mapper.Map<Book>(createBook);
        await _booksRepository.CreateAsync(bookEntity);

        var book = await _booksRepository.GetAsync(bookEntity.Id);
        return StatusCode((int)HttpStatusCode.Created, book);
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<Book>>> CreateBulkAsync([FromBody] IEnumerable<CreateBookDto> createBooks)
    {
        var bookEntities = _mapper.Map<IEnumerable<Book>>(createBooks);
        await _booksRepository.CreateAsync(bookEntities);

        var bookIds = bookEntities.Select(b => b.Id).ToList();
        return StatusCode((int)HttpStatusCode.Created, bookIds);
    }

    [HttpGet("bulk", Name = nameof(GetBulkAsync))]
    [TypeFilter(typeof(BookResultFilter))]
    public async Task<ActionResult<Book>> GetBulkAsync([FromQuery] IEnumerable<int> bookIds)
    {
        var entities = await _booksRepository.GetAsync(bookIds);
        if (entities?.Count() != bookIds.Count())
        {
            return NotFound();
        }

        return Ok(entities);
    }
}