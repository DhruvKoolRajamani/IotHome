using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IotHome.Controller.Sensor
{
    public class UltrasonicSensor
    {
        private GpioController Controller;
        private GpioPin TriggerPin { get; set; }
        private GpioPin EchoPin { get; set; }
        private const double SPEED_OF_SOUND_METERS_PER_SECOND = 343;

        public UltrasonicSensor(int triggerPin, int echoPin)
        {
            Controller = GpioController.GetDefault();

            if (Controller == null)
                return;

            this.TriggerPin = Controller.OpenPin(triggerPin);
            this.TriggerPin.SetDriveMode(GpioPinDriveMode.Output);

            this.EchoPin = Controller.OpenPin(echoPin);
            this.EchoPin.SetDriveMode(GpioPinDriveMode.Input);
        }

        private double LengthOfHighPulse
        {
            get
            {
                this.TriggerPin.Write(GpioPinValue.Low);
                Gpio.Sleep(500);
                this.TriggerPin.Write(GpioPinValue.High);
                Gpio.Sleep(10);
                this.TriggerPin.Write(GpioPinValue.Low);

                return Gpio.GetTimeUntilNextEdge(this.EchoPin, GpioPinValue.High, 100);
            }
        }

        public double Distance
        {
            get
            {
                return (SPEED_OF_SOUND_METERS_PER_SECOND / 2) * LengthOfHighPulse;
            }
        }

        public void Quit()
        {
            TriggerPin.Dispose();
            EchoPin.Dispose();
        }
    }
}
