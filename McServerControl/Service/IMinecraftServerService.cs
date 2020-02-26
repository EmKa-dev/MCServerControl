
using McServerControlAPI.Models;

namespace McServerControlAPI.Services
{
    public interface IMinecraftServerService
    {
        public ServerStatus ServerStatus { get; }

        void StartServer();

        void StopServer();

        bool IsInputPossible { get; }
    }
}