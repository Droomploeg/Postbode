using Droomploeg.DreamOps.Domain.Workers.Models;

namespace Droomploeg.DreamOps.Application.Workers.Services;

public interface INotificationService
{
    bool Update(DateTimeOffset dateTimeOffset);
    bool Cleanup(DateTimeOffset dateTimeOffset);
    void Remove(Guid Id);

    ICollection<Notification> GetAll();
}
