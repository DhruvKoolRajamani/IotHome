using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Timers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using IotHome.Controller.Sensor;
using System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IotHome.Controller
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static readonly String UrlPi = "http://192.168.1.12:5000/app";
        private static readonly String Url = "http://192.168.1.9:5000/app";

        HubConnection connection;

        private bool motorState = false;
        private bool boosterState = false;

        private double upperTankHeight = 0.0;
        private double lowerTankHeight = 0.0;

        private UltrasonicSensor upperTankSensor;
        private UltrasonicSensor lowerTankSensor;

        public MainPage()
        {
            this.InitializeComponent();

            connection = new HubConnectionBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.SetMinimumLevel(LogLevel.Debug);

                    loggingBuilder.ToString();
                })
                .WithUrl(UrlPi)
                .Build();

            connection.On<bool, bool>("Status", async (motorstate, boosterstate) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    OnServerCallForStatus(motorstate, boosterstate);
                });
            });

            connection.On<double, double>("Levels", async (upperLevel, lowerLevel) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    OnServerCallForLevels(upperLevel, lowerLevel);
                });
            });

            connection.Closed += async (error) =>
            {
                tbxConnect.Text = "Restarting Connection...";
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            upperTankSensor = new UltrasonicSensor(22, 27);
            lowerTankSensor = new UltrasonicSensor(6, 5);
        }

        private void OnServerCallForLevels(double upperLevel, double lowerLevel)
        {
            upperTankHeight = upperLevel;
            lowerTankHeight = lowerLevel;

            FillRectLevel(rectUpperTank, upperTankHeight);
            FillRectLevel(rectLowerTank, lowerTankHeight);
        }

        private void FillRectLevel(Rectangle rect, double height)
        {
            if (height <= 0.01)
                rect.Height = 1;
            else
                rect.Height = height * rect.MaxHeight;
        }

        private void OnServerCallForStatus(bool motorstate, bool boosterstate)
        {
            motorState = motorstate;
            boosterState = boosterstate;

            FillRectStatus(rectMotorStatus, motorState);
            FillRectStatus(rectBoosterStatus, boosterState);
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await connection.StartAsync();
                tbxConnect.Text = "Connected to Server";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tbxConnect.Text = ex.Message;
            }

            btnConnect.Visibility = Visibility.Collapsed;
            await connection.InvokeAsync("GetStates");

            while (true)
            {
                await GetLevel();
            }
        }

        private async Task GetLevel()
        {
            //timer = new Timer(1000);
            Thread.Sleep(5 * 1000);

            upperTankHeight = upperTankSensor.Distance;
            tbxUpperTankSensor.Text = upperTankHeight.ToString();

            lowerTankHeight = lowerTankSensor.Distance;
            tbxLowerTankSensor.Text = lowerTankHeight.ToString();

            await connection.InvokeAsync("SetTankLevels", upperTankHeight, lowerTankHeight);
        }

        private async void BtnMotorOn_Click(object sender, RoutedEventArgs e)
        {
            motorState = true;

            await connection.InvokeAsync("SetStates", motorState, boosterState);
            await connection.InvokeAsync("GetStates");
        }

        private async void BtnMotorOff_Click(object sender, RoutedEventArgs e)
        {
            motorState = false;

            await connection.InvokeAsync("SetStates", motorState, boosterState);
            await connection.InvokeAsync("GetStates");
        }

        private async void BtnBoosterOn_Click(object sender, RoutedEventArgs e)
        {
            boosterState = true;

            await connection.InvokeAsync("SetStates", motorState, boosterState);
            await connection.InvokeAsync("GetStates");
        }

        private async void BtnBoosterOff_Click(object sender, RoutedEventArgs e)
        {
            boosterState = false;

            await connection.InvokeAsync("SetStates", motorState, boosterState);
            await connection.InvokeAsync("GetStates");
        }

        private void FillRectStatus(Rectangle rect, bool state)
        {
            if (state)
            {
                rect.Fill = new SolidColorBrush(Windows.UI.Colors.Green);
            }
            else
            {
                rect.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
            }
        }
    }
}
