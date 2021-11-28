using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Api.BL;

namespace TodoList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly ILogger<TodoItemsController> _logger;
        private readonly IToDoService _toDoService;

        public TodoItemsController(TodoContext context, ILogger<TodoItemsController> logger, IToDoService toDoService)
        {
            _context = context;
            _logger = logger;
            this._toDoService = toDoService;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<IActionResult> GetTodoItems()
        {
            var results = await _toDoService.GetCompletedItemsAsync();
            return Ok(results);
        }

        // GET: api/TodoItems/...
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoItem(Guid id)
        {
            var result = await _toDoService.GetItemAsync(id);

            if (result.ComplexResult.ResultType == ResultType.NotFound)
            {
                return NotFound(result.ComplexResult.Message);
            }
            return Ok(result.Result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(Guid id, TodoItem todoItem)
        {
            var serviceResult = await _toDoService.SaveTodoItem(id, todoItem);

            switch (serviceResult.ComplexResult.ResultType)
            {
                case ResultType.OK:
                    return Ok(serviceResult.Result);
                case ResultType.BadRequest:
                case ResultType.UnknownError:
                    return BadRequest(serviceResult.ComplexResult.Message);
                case ResultType.NotFound:
                    return NotFound(serviceResult.ComplexResult.Message);
                case ResultType.NoContent:
                default:
                    return NoContent();
            }
        }

        // POST: api/TodoItems 
        [HttpPost]
        public async Task<IActionResult> PostTodoItem(TodoItem todoItem)
        {
            var serviceResult = await _toDoService.AddTodoItem(todoItem);


            switch (serviceResult.ComplexResult.ResultType)
            {
                case ResultType.OK:
                    return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
                case ResultType.BadRequest:
                case ResultType.UnknownError:
                    return BadRequest(serviceResult.ComplexResult.Message);
                case ResultType.NotFound:
                    return NotFound(serviceResult.ComplexResult.Message);
                case ResultType.NoContent:
                default:
                    return NoContent();
            }
        }

    }
}
