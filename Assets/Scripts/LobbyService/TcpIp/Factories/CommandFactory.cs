using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using LobbyService.TcpIp.Commands;
using LobbyService.TcpIp.Commands.Common;
using LobbyService.TcpIp.Interfaces;

namespace LobbyService.TcpIp.Factories
{
    public static class CommandFactory
    {
        #region ImportsInfo

        private class ImportInfo
        {
            [ImportMany]
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public IEnumerable<Lazy<ICommand, CommandMetadata>> Commands { get; set; } = Enumerable.Empty<Lazy<ICommand, CommandMetadata>>();
        }

        #endregion
        
        private static readonly ImportInfo Info = new();

        static CommandFactory()
        {
            Assembly[] assemblies = { typeof(ICommand).Assembly };
            ContainerConfiguration configuration = new();
            try
            {
                configuration = configuration.WithAssemblies(assemblies);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to load CommandFactory");
                throw;
            }
        
            using CompositionHost container = configuration.CreateContainer();
            container.SatisfyImports(Info);   
        }

        public static async Task<Result<string>> StartCommandAsync(CommandType type, object content, NetworkStream stream, CancellationToken token)
        {
            ICommand command = GetCommand(type);

            if (command != null)
            {
                return await command.Execute(stream, content, token);
            }

            Logger.Log($"Command {type} not found", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
            return Result.Fail(new Error("Command not found"));
        }
        
        private static ICommand GetCommand(CommandType type)
        {
            return Info.Commands.FirstOrDefault(x => x.Metadata.Type == type)?.Value;
        }
    }
}