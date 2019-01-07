using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace IotHome.server.Hubs
{
    public class IotHomeHub : Hub
    {
        private readonly IHomeRepo _repository;

        public IotHomeHub(IHomeRepo repository)
        {
            _repository = repository;
        }

        public async Task GetStates()
        {
            await Clients.Caller.SendAsync("Status", _repository.MotorState, _repository.BoosterState);
        }

        public async Task ErrorState(string message)
        {
            await Clients.Others.SendAsync("ErrorStatus", message);
        }

        public async Task SetStates(bool motorState, bool boosterState)
        {
            _repository.MotorState = motorState;
            _repository.BoosterState = boosterState;

            await Clients.Others.SendAsync("Status", _repository.MotorState, _repository.BoosterState);
        }

        public async Task SetTankLevels(double uppertank, double lowertank)
        {
            _repository.UpperTank = uppertank;
            _repository.LowerTank = lowertank;

            await Clients.Caller.SendAsync("Levels", _repository.UpperTank, _repository.LowerTank);
        }

        public async Task GetTankLevels()
        {
            await Clients.Caller.SendAsync("Levels", _repository.UpperTank, _repository.LowerTank);
        }
    }
}