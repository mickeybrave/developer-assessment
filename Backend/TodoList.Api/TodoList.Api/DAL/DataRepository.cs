using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoList.Api.DAL
{

    public interface IToDoDataRepository
    {
        Task<IEnumerable<TodoItem>> GetItemsAsync(Func<TodoItem, bool> filter);
        Task<TodoItem> GetItemAsync(Guid id);
        Task SaveTodoItem(TodoItem todoItem);
        Task AddTodoItem(TodoItem todoItem);
    }

    public class ToDoDataRepository : IToDoDataRepository
    {
        private readonly TodoContext _context;

        public ToDoDataRepository(TodoContext context)
        {
            this._context = context;
        }

        public Task<TodoItem> GetItemAsync(Guid id)
        {
            return _context.TodoItems.FirstOrDefaultAsync(f => f.Id == id);
        }

       

        public Task<IEnumerable<TodoItem>> GetItemsAsync(Func<TodoItem, bool> filter)
        {
            return Task.Run(() => _context.TodoItems.Where(filter));
        }


        public async Task SaveTodoItem(TodoItem todoItem)
        {
            try
            {
                _context.Entry(todoItem).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        public async Task AddTodoItem(TodoItem todoItem)
        {
            try
            {
                _context.TodoItems.Add(todoItem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

    }



}
