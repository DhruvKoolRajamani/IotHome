using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UltrasonicSensor;
using Pumps;
using System.Threading;
using System.Diagnostics;
using Levels;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IotHome.BackgroundLevelController
{
    public sealed class StartupTask : IBackgroundTask
    {
        private Pump Motor;
        private Pump Booster;

        private DiscreteLevel UpperTankLevel;
        private DiscreteLevel LowerTankLevel;

        private static readonly String UrlPi = "http://192.168.1.12:5000/app";

        HubConnection connection;

        Timer t = null;

        private BackgroundTaskDeferral _Deferral;
        private float upperTankDepth = 0.0f;
        private float lowerTankDepth = 0.0f;

        private void InitLevels()
        {
            UpperTankLevel = new DiscreteLevel(26, 19, 13, 6);
            UpperTankLevel.Init();
            UpperTankLevel.EventLevels += UpperTankLevel_EventLevels;

            LowerTankLevel = new DiscreteLevel(21, 20, 16, 12);
            LowerTankLevel.Init();
            LowerTankLevel.EventLevels += LowerTankLevel_EventLevels;
        }

        private void InitPumps()
        {
            Motor = new Pump(17, "Motor"); //Red
            Motor.Init();
            Motor.EventPumpStatus += EventPumpStatus; ;

            Booster = new Pump(27, "Booster"); //Green
            Booster.Init();
            Booster.EventPumpStatus += EventPumpStatus;
        }

        private async void EventPumpStatus(object sender, PumpStatusEventArgs e)
        {
            await connection.InvokeAsync("SetStates", Motor.State, Booster.State);
        }

        private async void InitSignalR()
        {
            connection = new HubConnectionBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.SetMinimumLevel(LogLevel.Debug);

                    loggingBuilder.ToString();
                })
                .WithUrl(UrlPi)
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<bool, bool>("Status", (motorstate, boosterstate) =>
            {
                Debug.WriteLine($"OnConnect motor:{motorstate} booster:{boosterstate}");
            });

            await connection.StartAsync();
            await connection.InvokeAsync("GetStates");
        }
        
        private async void LowerTankLevel_EventLevels(object sender, LevelsEventArgs e)
        {
            lowerTankDepth = e.Depth / 100.0f;
            Debug.WriteLine($"Lower Depth: {e.Depth}");
            await connection.InvokeAsync("SetTankLevels", upperTankDepth, lowerTankDepth);
        }

        private async void UpperTankLevel_EventLevels(object sender, LevelsEventArgs e)
        {
            upperTankDepth = e.Depth / 100.0f;
            Debug.WriteLine($"Upper Depth: {e.Depth}");
            await connection.InvokeAsync("SetTankLevels", upperTankDepth, lowerTankDepth);
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
            _Deferral = taskInstance.GetDeferral();

            InitLevels();
            InitPumps();
            InitSignalR();

            t = new Timer(tCallback, null, 0, 5000);
        }

        private void tCallback(object state)
        {
            LowerTankLevel.Ping();

            UpperTankLevel.Ping();
        }
    }
}
