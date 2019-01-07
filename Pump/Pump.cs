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
        public bool Status { get; set; }

        public PumpStatusEventArgs(bool status)
        {
            Status = status;
        }
    }

    public class Pump
    {
        private bool controllerFlag = false;
        private bool state = false;

        public bool State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        private int pinId;

        private GpioController gpioController;

        private GpioPin gpioPin;

        private GpioOpenStatus gpioOpenStatus;

        private GpioPinValue pinValue;

        public event EventHandler<PumpStatusEventArgs> EventPumpStatus = delegate { };

        public Pump(int pinid, bool _state)
        {
            pinId = pinid;

            state = _state;
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

                    if (pinValue == GpioPinValue.High)
                    {
                        state = false;
                    }
                    else
                        state = true;
                }
                else
                    state = false;
            }
            else
                state = false;
        }

        public void Actuate(bool _state)
        {
            if (controllerFlag)
            {
                if (gpioOpenStatus == GpioOpenStatus.PinOpened)
                {
                    if (_state == false)
                    {
                        pinValue = GpioPinValue.High;
                    }
                    else
                        pinValue = GpioPinValue.Low;

                    this.gpioPin.SetDriveMode(GpioPinDriveMode.Output);
                    this.gpioPin.Write(pinValue);
                    Debug.WriteLine($"Actuate: {this.gpioPin.PinNumber} ==> {pinValue}");
                }
                state = _state;
            }
        }
    }
}
