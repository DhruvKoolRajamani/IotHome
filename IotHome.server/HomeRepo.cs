using System.Collections.Generic;
using Bifrost.Devices.Gpio;
using Bifrost.Devices.Gpio.Abstractions;
using Bifrost.Devices.Gpio.Core;
using IotHome.server.Hubs;

namespace IotHome.server
{
    public interface IHomeRepo
    {
        bool MotorState { get; set; }
        bool BoosterState { get; set; }

        void Add();
        void Check();
    }
    
    public class HomeRepo : IHomeRepo
    {
        // private IGpioController _IGpioController;
        // private GpioPin gpioPin;

        private bool _motorState, _boosterState;
        private readonly int motorPinId = 17;
        private readonly int boosterPinId = 18;

        enum usedPins
        {
            MotorPin = 17,
            BoosterPin = 18
        }

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

        private void ReadPin()
        {
            
        }

        void IHomeRepo.Check()
        {

        }

        void IHomeRepo.Add()
        {

        }
    }
}