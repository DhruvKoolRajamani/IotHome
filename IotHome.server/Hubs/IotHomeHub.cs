using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace IotHome.server.Hubs
{
    public interface IHomeRepo
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

        private readonly IHomeRepo _repository;

        public bool MotorState { get => motorState; set => motorState = value; }
        public bool BoosterState { get => boosterState; set => boosterState = value; }

        public IotHomeHub(IHomeRepo repository)
        {
            _repository = repository;
            _repository.MotorState = repository.MotorState;
            _repository.BoosterState = repository.BoosterState;
        }

        public async Task Init() => await Clients.Caller.SendAsync("status", _repository.MotorState, _repository.BoosterState);

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

            await Clients.Others.SendAsync("status", _repository.MotorState, _repository.BoosterState);
        }
    }
}