namespace Droomploeg.DreamOps.Application.Common;

public interface ICommandDispatcher
{
    Task<bool> SendAsync(ICommand command, CancellationToken cancellationToken = default);
}
