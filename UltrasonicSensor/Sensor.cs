using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices;
using System.Threading.Tasks;

namespace UltrasonicSensor
{
    public class DistanceEventArgs : EventArgs
    {
        public string Distance { get; set; }
        public float Depth { get; set; }

        public DistanceEventArgs(string distance, float depth)
        {
            Distance = distance;
            Depth = depth;
        }
    }
    
    public class GpioStatusEventArgs : EventArgs
    {
        public string Status { get; set; }

        public GpioStatusEventArgs(string status)
        {
            Status = status;
        }
    }

    public class Sensor
    {
        #region MEMBER_VARIABLES
        private bool controllerFlag;

        private GpioPinEdge triggerEdge;

        private int _timer;
        private int nTimes = 1;

        private int TRIGGER;
        private int ECHO;

        private long ns = 0;

        private double ticksPerSecond = 0.0;
        
        private GpioController gpioController;

        private GpioPin gpioTrigger;
        private GpioPin gpioEcho;

        private GpioOpenStatus gpioOpenStatusTrigger;
        private GpioOpenStatus gpioOpenStatusEcho;

        private const float SPEED_OF_SOUND_METERS_PER_SECOND = 343.0f;

        private Stopwatch _sw;

        private List<float> listDistances;

        private Timer timer = null;

        private ManualResetEventSlim mre;
        #endregion

        #region EVENT_HANDLING
        public event EventHandler<DistanceEventArgs> EventDistance = delegate { };
        public event EventHandler<GpioStatusEventArgs> EventGpioStatus = delegate { };
        #endregion

        #region CONSTRUCTORS
        public Sensor(int trigger, int echo, int timer = 1000)
        {
            TRIGGER = trigger;
            ECHO = echo;

            _timer = timer;

            _sw = new Stopwatch();

            ns = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
            ticksPerSecond = 1.0 / Stopwatch.Frequency;

            listDistances = new List<float>();

            mre = new ManualResetEventSlim(false);
        }

        ~Sensor()
        {
            this.gpioEcho.ValueChanged -= gpioEcho_ValueChanged;

            mre.Dispose();

            gpioTrigger.Dispose();
            gpioEcho.Dispose();

            controllerFlag = false;
        }
        #endregion

        public void Init()
        {
            gpioController = GpioController.GetDefault();

            if (gpioController == null)
                controllerFlag = false;
            else
                controllerFlag = true;

            if (controllerFlag)
            {
                if (this.gpioController.TryOpenPin(TRIGGER, GpioSharingMode.Exclusive, out gpioTrigger, out gpioOpenStatusTrigger))
                {
                    this.gpioTrigger.SetDriveMode(GpioPinDriveMode.Output);
                    EventGpioStatus(this, new GpioStatusEventArgs($"TRIGGER {TRIGGER} Status: {gpioOpenStatusTrigger} Drive Mode: {gpioTrigger.GetDriveMode()}"));
                    this.gpioTrigger.Write(GpioPinValue.Low);
                }

                if (this.gpioController.TryOpenPin(ECHO, GpioSharingMode.Exclusive, out gpioEcho, out gpioOpenStatusEcho))
                {
                    this.gpioEcho.SetDriveMode(GpioPinDriveMode.Input);
                    EventGpioStatus(this, new GpioStatusEventArgs($"ECHO {ECHO} Status: {gpioOpenStatusEcho} Drive Mode: {gpioEcho.GetDriveMode()}"));
                    this.gpioEcho.ValueChanged += gpioEcho_ValueChanged;
                }
            }
            else
            {
                EventGpioStatus(this, new GpioStatusEventArgs($"No Gpio provider!"));
            }
            
        }

        private void GpioTrigger_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            triggerEdge = args.Edge;
        }

        public void Ping()
        {
            if (timer == null)
                timer = new Timer(CheckStatus, null, Timeout.Infinite, _timer);
            
            timer.Change(0, _timer);
        }

        public void CheckStatus(Object stateInfo)
        {
            Debug.WriteLine($"Controller {controllerFlag} nTimes:{nTimes}");
            if (nTimes < 6)
            {
                if (controllerFlag)
                {
                    Measure();
                }
                else // to run tests on local machine
                    listDistances.Add((float)new Random().NextDouble());
                nTimes++;
            }
            else
            {
                timer.Change(Timeout.Infinite,Timeout.Infinite);
                nTimes = 1;
                if (listDistances.Count == 0)
                {
                    EventDistance(this, new DistanceEventArgs($"Error", -1));
                }else
                {
                    int bufferSize = 1;

                    //foreach (float distance in listDistances)
                    listDistances.Sort();
                    //listDistances.RemoveRange(0, bufferSize);
                    //listDistances.RemoveRange(listDistances.Count - bufferSize, bufferSize);

                    //listDistances.Remove(listDistances.Max());
                    //listDistances.Remove(listDistances.Min());

                    EventDistance(this, new DistanceEventArgs($"Distance", listDistances.Average()));
                    //Debug.WriteLine($"------ {listDistances.Average()} ------");
                    //EventDistance(this, new DistanceEventArgs($"Distance", listDistances.Last()));
                    listDistances.Clear();
                }
            }
        }

        private void Measure()
        {
            Debug.WriteLine("Ping");
            this.gpioTrigger.Write(GpioPinValue.High);

            this.delayMicro(10);

            this.gpioTrigger.Write(GpioPinValue.Low);
            //_sw.Restart();
        }

        private void gpioEcho_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            //Debug.WriteLine($"Echo: {args.Edge}");

            if (args.Edge == GpioPinEdge.RisingEdge)
                _sw.Start();
            else
            {
                _sw.Stop();
                double lTicks = _sw.ElapsedTicks * ticksPerSecond;
                _sw.Reset();                
                listDistances.Add(SPEED_OF_SOUND_METERS_PER_SECOND * 100.0f * (float)lTicks / (2));

                Debug.WriteLine($"Value {nTimes} - {listDistances.Last()} - Time: {lTicks}");
            }
        }

        private void delayMicro(double delay)
        {
            mre.Wait(
                TimeSpan.FromMilliseconds(
                    (delay / 1000.0)
                )
            );
        }
    }
}
