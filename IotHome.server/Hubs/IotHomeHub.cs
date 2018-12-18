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

        public async Task Init() => await Clients.Caller.SendAsync("status", _repository.MotorState, _repository.BoosterState);

        public async Task Controller(bool motorState, bool boosterState)
        {
            _repository.MotorState = motorState;
            _repository.BoosterState = boosterState;

            await Clients.Others.SendAsync("status", _repository.MotorState, _repository.BoosterState);
        }
    }
}