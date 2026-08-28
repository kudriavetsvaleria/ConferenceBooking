using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Interfaces;

public interface IServiceRepository : IRepository<Service>
{
    Task<List<Service>> GetAllAsync();
    Task UpdateAsync(Service service);
    Task DeleteAsync(int id);
}