using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using LobbyService.TcpIp.Commands;

namespace LobbyService.TcpIp.Interfaces
{
    public interface ICommand
    {
        CommandType? Type { get; }

        Task<Result<string>> Execute(NetworkStream stream, object content, CancellationToken token = default);
    }
}