using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IotHome.Controller.Sensor
{
    public class Gpio
    {
        private static ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private static Stopwatch stopWatch = new Stopwatch();

        public static void Sleep(int delayMicroseconds)
        {
            manualResetEvent.WaitOne(
                TimeSpan.FromMilliseconds((double)delayMicroseconds / 1000d));
        }

        public static double GetTimeUntilNextEdge(GpioPin pin, GpioPinValue edgeToWaitFor, int maximumTimeToWaitInMilliseconds)
        {
            var t = Task.Run(() =>
            {
                stopWatch.Reset();

                while (pin.Read() != edgeToWaitFor) { };

                stopWatch.Start();

                while (pin.Read() == edgeToWaitFor) { };

                stopWatch.Stop();

                return stopWatch.Elapsed.TotalSeconds;
            });

            bool isCompleted = t.Wait(TimeSpan.FromMilliseconds(maximumTimeToWaitInMilliseconds));

            if (isCompleted)
            {
                return t.Result;
            }
            else
            {
                return -1d;
            }
        }
    }
}
