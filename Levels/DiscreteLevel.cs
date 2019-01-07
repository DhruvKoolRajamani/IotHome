﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using System.Threading;

namespace Levels
{
    public class LevelsEventArgs : EventArgs
    {
        public int Level { get; set; }
        public float Depth { get; set; }

        public LevelsEventArgs(int level)
        {
            Level = level;
        }

        public void GetLevel()
        {
            Depth = (100.0f / 3.0f) * Level;
        }
    }

    public class DiscreteLevel
    {
        #region MEMBER_VARIABLES
        private bool controllerFlag = false;

        private int TRIGGER;
        private int[] LevelPins;

        private GpioController gpioController;

        private GpioPin gpioTrigger;
        private GpioPin[] gpioLevels;

        private GpioOpenStatus[] gpioOpenStatus;

        private int n = 0;

        private ManualResetEventSlim mre;

        List<int> check;
        #endregion

        public event EventHandler<LevelsEventArgs> EventLevels = delegate { };

        public DiscreteLevel(int trigger, int level_1, int level_2, int level_3)
        {
            gpioController = GpioController.GetDefault();

            if (gpioController != null)
                controllerFlag = true;
            else
                controllerFlag = false;

            TRIGGER = trigger;

            LevelPins = new int[3];

            LevelPins[0] = level_1;
            LevelPins[1] = level_2;
            LevelPins[2] = level_3;

            gpioOpenStatus = new GpioOpenStatus[4];
            gpioLevels = new GpioPin[3];

            mre = new ManualResetEventSlim();

            check = new List<int>();
        }

        public void Init()
        {
            if (controllerFlag)
            {
                if (gpioController.TryOpenPin(TRIGGER, GpioSharingMode.Exclusive, out gpioTrigger, out gpioOpenStatus[0]))
                {
                    gpioTrigger.SetDriveMode(GpioPinDriveMode.Output);
                    gpioTrigger.Write(GpioPinValue.Low);
                }

                for (int i=0; i<3; i++)
                {
                    if (gpioController.TryOpenPin(LevelPins[i], GpioSharingMode.Exclusive, out gpioLevels[i], out gpioOpenStatus[i+1]))
                    {
                        gpioLevels[i].SetDriveMode(GpioPinDriveMode.Input);
                        gpioLevels[i].ValueChanged += LevelValueChanged;
                    }
                }
            }
            else
                Debug.WriteLine("ERROR, No Controller");

            EventLevels(this, new LevelsEventArgs(0));
        }

        public void Ping()
        {
            n = 0;
            check.Add(n);

            Debug.WriteLine($"Trigger");

            gpioTrigger.Write(GpioPinValue.High);
            delayMilli(100);
            gpioTrigger.Write(GpioPinValue.Low);

            n = check.Max();
            check.Clear();
            EventLevels(this, new LevelsEventArgs(n));
        }

        private void LevelValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                if (sender.PinNumber == LevelPins[0])
                {
                    n = 1;
                    check.Add(n);
                }

                sender.DebounceTimeout = TimeSpan.FromMilliseconds(50);

                if (sender.PinNumber == LevelPins[1])
                {
                    n = 2;
                    check.Add(n);
                    sender.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                }

                if (sender.PinNumber == LevelPins[2])
                {
                    n = 3;
                    check.Add(n);
                    sender.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                }
            }
        }

        private void delayMilli(double delay)
        {
            mre.Wait(
                TimeSpan.FromMilliseconds(
                    (delay)
                )
            );
        }
    }
}
