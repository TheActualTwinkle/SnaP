using System;
using System.Composition;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using LobbyService.TcpIp.Interfaces;
using Newtonsoft.Json;

// ReSharper disable global UnusedType.Global
namespace LobbyService.TcpIp.Commands
{
    public class CommandBase
    {
        public CommandBase(CommandType? type, object content)
        {
            Type = type;
            Content = content;
        }

        public CommandType? Type { get; }
        public object Content { get; }
    }
    
    [Export(typeof(ICommand))]
    [ExportMetadata(nameof(Type), CommandType.PostLobbyInfo)]
    public class PostLobbyInfoCommand : ICommand
    {
        public CommandType? Type => CommandType.PostLobbyInfo;
        
        public async Task<Result<string>> Execute(NetworkStream stream, object content, CancellationToken token = default)
        {
            CommandBase command = new(Type, content);
            await NetworkStreamReaderWriter.WriteAsync(stream, JsonConvert.SerializeObject(command));
            
            return Result.Ok();
        }
    }
    
    [Export(typeof(ICommand))]
    [ExportMetadata(nameof(Type), CommandType.GetStatus)]
    public class GetStatusCommand : ICommand
    {
        public CommandType? Type => CommandType.GetStatus;
        
        private const string GetStatusResponse = "OK";
        
        public async Task<Result<string>> Execute(NetworkStream stream, object content, CancellationToken token = default)
        {
            CommandBase command = new(Type, content);
            await NetworkStreamReaderWriter.WriteAsync(stream, JsonConvert.SerializeObject(command));

            string response = await NetworkStreamReaderWriter.ReadAsync(stream);

            return response.Contains(GetStatusResponse) ? Result.Ok() : Result.Fail(new Error("Failed to get status"));
        }
    }
    
    [Export(typeof(ICommand))]
    [ExportMetadata(nameof(Type), CommandType.GetLobbyGuids)]
    public class GetLobbyGuidsCommand : ICommand
    {
        public CommandType? Type => CommandType.GetLobbyGuids;
        
        public async Task<Result<string>> Execute(NetworkStream stream, object content, CancellationToken token = default)
        {
            CommandBase command = new(Type, content);
            await NetworkStreamReaderWriter.WriteAsync(stream, JsonConvert.SerializeObject(command));

            string response = await NetworkStreamReaderWriter.ReadAsync(stream);

            return Result.Ok(response);
        }
    }
    
    [Export(typeof(ICommand))]
    [ExportMetadata(nameof(Type), CommandType.GetLobbyInfo)]
    public class GetLobbyInfoCommand : ICommand
    {
        public CommandType? Type => CommandType.GetLobbyInfo;
        
        public async Task<Result<string>> Execute(NetworkStream stream, object content, CancellationToken token = default)
        {
            CommandBase command = new(Type, content);
            await NetworkStreamReaderWriter.WriteAsync(stream, JsonConvert.SerializeObject(command));

            string response = await NetworkStreamReaderWriter.ReadAsync(stream);

            return Result.Ok(response);
        }
    }
    
    [Export(typeof(ICommand))]
    [ExportMetadata(nameof(Type), CommandType.Close)]
    public class CloseCommand : ICommand
    {
        public CommandType? Type => CommandType.Close;
        
        public async Task<Result<string>> Execute(NetworkStream stream, object content, CancellationToken token = default)
        {
            CommandBase command = new(Type, content);
            await NetworkStreamReaderWriter.WriteAsync(stream, JsonConvert.SerializeObject(command));

            return Result.Ok();
        }
    }
}