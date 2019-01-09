using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Pumps
{
    public class PumpStatusEventArgs : EventArgs
    {
        public string Status { get; set; }
        public bool State { get; set; }

        public PumpStatusEventArgs(string status, bool state)
        {
            Status = status;
            State = state;
        }
    }

    public class Pump
    {
        private bool controllerFlag = false;
        private bool _state = false;

        public bool State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        private int pinId;

        private GpioController gpioController;

        private GpioPin gpioPin;

        private GpioOpenStatus gpioOpenStatus;

        private GpioPinValue pinValue;
        
        public event EventHandler<PumpStatusEventArgs> EventPumpStatus = delegate { };

        private string _name = "";

        public Pump(int pinid, string name)
        {
            pinId = pinid;
            _name = name;
        }

        public void Init()
        {
            gpioController = GpioController.GetDefault();

            if (gpioController == null)
                controllerFlag = false;
            else
                controllerFlag = true;

            GetState();

        }

        private void GetState()
        {
            if (controllerFlag)
            {
                if (this.gpioController.TryOpenPin(pinId, GpioSharingMode.Exclusive, out gpioPin, out gpioOpenStatus))
                {
                    this.gpioPin.SetDriveMode(GpioPinDriveMode.Input);
                    this.pinValue = this.gpioPin.Read();
                    Debug.WriteLine($"pinId - {pinId}; {pinValue}");
                    if (pinValue == GpioPinValue.Low )
                        _state = true;

                    this.gpioPin.SetDriveMode(GpioPinDriveMode.Output);
                }
            }
        }

        public void Actuate(bool state)
        {
            if (controllerFlag)
            {
                if (gpioOpenStatus == GpioOpenStatus.PinOpened)
                {
                    if (state == false)
                        pinValue = GpioPinValue.High;
                    else
                        pinValue = GpioPinValue.Low;

                    this.gpioPin.Write(pinValue);
                    _state = state;
                    Debug.WriteLine($"Actuate: {_name} ==> {pinValue}");
                    EventPumpStatus(this, new PumpStatusEventArgs($"Pump: {_name} is switching : {state}", state));
                }
            }
        }
    }
}
