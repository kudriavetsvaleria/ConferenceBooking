namespace ConferenceBooking.Application.Interfaces;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
}