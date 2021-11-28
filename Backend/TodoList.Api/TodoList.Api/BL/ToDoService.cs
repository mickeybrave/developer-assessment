using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Api.DAL;

namespace TodoList.Api.BL
{
    public class ComplexResult
    {
        public string Message { get; set; }
        public ResultType ResultType { get; set; }
    }
    public enum ResultType
    {
        OK,
        BadRequest,
        NotFound,
        NoContent,
        UnknownError
    }
    public class DataResult<T>
    {
        public T Result { get; private set; }
        public ComplexResult ComplexResult { get; private set; }

        public DataResult(T result, ComplexResult complexResult)
        {
            Result = result;
            ComplexResult = complexResult;
        }
    }
    public interface IToDoService
    {
        Task<IEnumerable<TodoItem>> GetCompletedItemsAsync();

        Task<DataResult<TodoItem>> GetItemAsync(Guid id);

        Task<DataResult<TodoItem>> SaveTodoItem(Guid id, TodoItem todoItem);

        Task<DataResult<TodoItem>> AddTodoItem(TodoItem todoItem);
    }

    public class ToDoService : IToDoService
    {
        private readonly IToDoDataRepository _dataRepository;

        public ToDoService(IToDoDataRepository dataRepository)
        {
            this._dataRepository = dataRepository;
        }



        public Task<IEnumerable<TodoItem>> GetCompletedItemsAsync()
        {
            return Task.Run(() => _dataRepository.GetItemsAsync(x => !x.IsCompleted));
        }

        public async Task<DataResult<TodoItem>> GetItemAsync(Guid id)
        {
            var res = await _dataRepository.GetItemAsync(id);

            if (res == null)
            {
                return new DataResult<TodoItem>(null, new ComplexResult { Message = $"Id={id} was not found in the system", ResultType = ResultType.NotFound });
            }

            return new DataResult<TodoItem>(res, new ComplexResult { Message = null, ResultType = ResultType.OK });
        }


        public async Task<DataResult<TodoItem>> SaveTodoItem(Guid id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return new DataResult<TodoItem>(null, new ComplexResult { Message = $"Id={id} provided is not the id in ToDo item provided (id={todoItem.Id})", ResultType = ResultType.BadRequest });
            }
            var itemFoundInDb = await _dataRepository.GetItemAsync(id);

            if (itemFoundInDb == null)
            {
                return new DataResult<TodoItem>(null, new ComplexResult { Message = $"Id={id} was not found in the system", ResultType = ResultType.NotFound });
            }

            try
            {
                await _dataRepository.SaveTodoItem(itemFoundInDb);
            }
            catch (Exception ex)
            {
                return new DataResult<TodoItem>(null, new ComplexResult { Message = ex.Message, ResultType = ResultType.UnknownError });
            }
            return new DataResult<TodoItem>(null, new ComplexResult { Message = null, ResultType = ResultType.NoContent });
        }

        public async Task<DataResult<TodoItem>> AddTodoItem(TodoItem todoItem)
        {
            if (string.IsNullOrEmpty(todoItem?.Description))
            {
                return new DataResult<TodoItem>(null, new ComplexResult { Message = "Description is required", ResultType = ResultType.BadRequest });
            }

            var incompleteItemsWithSameDesciption = await _dataRepository.GetItemsAsync(x => x.Description.ToLowerInvariant() == todoItem.Description.ToLowerInvariant() && !x.IsCompleted);

            if (incompleteItemsWithSameDesciption.Any())
            {
                return new DataResult<TodoItem>(null, new ComplexResult { Message = "Description already exists", ResultType = ResultType.BadRequest });
            }

            try
            {
                await _dataRepository.AddTodoItem(todoItem);
            }
            catch (Exception ex)
            {
                return new DataResult<TodoItem>(null, new ComplexResult { Message = ex.Message, ResultType = ResultType.UnknownError });
            }

            return new DataResult<TodoItem>(todoItem, new ComplexResult { Message = null, ResultType = ResultType.OK });

        }

    }
}
