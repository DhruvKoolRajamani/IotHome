using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace IotHome.server.Hubs
{
    public interface IHomeHub
    {
        bool MotorState { get; set; }
        bool BoosterState { get; set; }

        void Add(bool motorState, bool boosterState);
        void Check();
    }
    public class IotHomeHub : Hub
    {
        private bool motorState;
        private bool boosterState;

        private readonly IHomeHub _repository;

        public bool MotorState { get => motorState; set => motorState = value; }
        public bool BoosterState { get => boosterState; set => boosterState = value; }

        public IotHomeHub(IHomeHub repository)
        {
            _repository = repository;
            motorState = repository.MotorState;
            boosterState = repository.BoosterState;
            _repository.Check();
        }

        public async Task Init() => await Clients.All.SendAsync("status", _repository.MotorState, _repository.BoosterState);// return true;

        public async Task Controller(int stateCase)
        {
            switch (stateCase)
            {
                case 1:
                    MotorState = false;
                    BoosterState = false;
                    _repository.Add(MotorState, BoosterState);
                    break;

                case 2:
                    MotorState = false;
                    BoosterState = true;
                    _repository.Add(MotorState, BoosterState);
                    break;

                case 3:
                    MotorState = true;
                    BoosterState = false;
                    _repository.Add(MotorState, BoosterState);
                    break;

                case 4:
                    MotorState = true;
                    BoosterState = true;
                    _repository.Add(MotorState, BoosterState);
                    break;
            }

            await Clients.All.SendAsync("status", _repository.MotorState, _repository.BoosterState);
        }
    }
}