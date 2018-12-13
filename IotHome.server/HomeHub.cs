using IotHome.server.Hubs;

namespace IotHome.server
{
    public class HomeHub : IHomeHub
    {
        //private readonly GpioController _controller = new GpioController();
        //private GpioOpenStatus openStatus;
        //private GpioPin gpioPin;

        private bool _motorState, _boosterState;
        private readonly int motorPinId = 17;
        private readonly int boosterPinId = 18;

        public HomeHub()
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

        void IHomeHub.Check()
        {
            //if (_controller.TryOpenPin(17, gpioPin.SharingMode, out GpioPin motorPin, out openStatus))
            //{
            //    if (openStatus == GpioOpenStatus.PinOpened)
            //    {
            //        motorPin.SetDriveMode(GpioPinDriveMode.Input);
            //        if(motorPin.Read() == GpioPinValue.High)
            //            MotorState = true;
            //        else
            //            MotorState = false;
            //    }
            //    else
            //    {
            //        Console.WriteLine("Pin %d is closed", 17);
            //    }
            //}
            //else if (_controller.TryOpenPin(18, gpioPin.SharingMode, out GpioPin boosterPin, out openStatus))
            //{
            //    if (openStatus == GpioOpenStatus.PinOpened)
            //    {
            //        boosterPin.SetDriveMode(GpioPinDriveMode.Input);
            //        if (motorPin.Read() == GpioPinValue.High)
            //            BoosterState = true;
            //        else
            //            BoosterState = false;
            //    }
            //    else
            //    {
            //        Console.WriteLine("Pin %d is closed", 18);
            //    }
            //}
        }

        void IHomeHub.Add(bool motorState, bool boosterState)
        {
            //if (_controller.TryOpenPin(gpioPinId, gpioPin.SharingMode, out GpioPin pin, out openStatus))
            //{
            //    pin.SetDriveMode(GpioPinDriveMode.Output);
            //    pin.Write(GpioPinValue.High);
            //    Console.WriteLine("Opened Pin: " + gpioPinId);
            //}

            _motorState = motorState;
            _boosterState = boosterState;
        }
    }
}