using System.Collections.Generic;
using System.Threading;
using IotHome.server.Hubs;

namespace IotHome.server
{
    public interface IHomeRepo
    {
        bool MotorState { get; set; }
        bool BoosterState { get; set; }

        double UpperTank { get; set; }
        double LowerTank { get; set; }
    }
    
    public class HomeRepo : IHomeRepo
    {
        private bool _motorState, _boosterState;

        private double _upperTank, _lowerTank;

        public HomeRepo()
        {
            _motorState = MotorState;
            _boosterState = BoosterState;
        }

        public bool MotorState
        {
            get => _motorState;
            set => _motorState = value;
        }

        public bool BoosterState
        {
            get => _boosterState;
            set => _boosterState = value;
        }

        public double UpperTank
        {
            get => _upperTank;
            set => _upperTank = value;
        }

        public double LowerTank
        {
            get => _lowerTank;
            set => _lowerTank = value;
        }
    }
}