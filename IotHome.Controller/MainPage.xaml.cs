using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using System.Threading;
using UltrasonicSensor;

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

        private Sensor upperTankSensor;
        private Sensor lowerTankSensor;

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
                tbkConnect.Text = "Restarting Connection...";
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }

        private async void LowerTankSensor_EventGpioStatus(object sender, GpioStatusEventArgs e)
        {
            await lbLowerTankStatus.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lbLowerTankStatus.Items.Add($"Status : {e.Status}");
            });
        }

        private async void UpperTankSensor_EventGpioStatus(object sender, GpioStatusEventArgs e)
        {
            await lbUpperTankStatus.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lbUpperTankStatus.Items.Add($"Status : {e.Status}");
            });
        }

        private async void LowerTankSensor_EventDistance(object sender, DistanceEventArgs e)
        {
            await tbkLowerTankSensor.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbkLowerTankSensor.Text = $"Status : {e.Depth}";
                lowerTankHeight = e.Depth;
            });
        }

        private async void UpperTankSensor_EventDistance(object sender, DistanceEventArgs e)
        {
            await tbkUpperTankSensor.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbkUpperTankSensor.Text = $"Status : {e.Depth}";
                upperTankHeight = e.Depth;
            });
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
                tbkConnect.Text = "Connected to Server";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tbkConnect.Text = ex.Message;
            }

            btnConnect.Visibility = Visibility.Collapsed;
            await connection.InvokeAsync("GetStates");
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

        private async void BtnSensor_Click(object sender, RoutedEventArgs e)
        {
            upperTankSensor = new Sensor(22, 27);
            upperTankSensor.Init();
            upperTankSensor.EventDistance += UpperTankSensor_EventDistance;
            upperTankSensor.EventGpioStatus += UpperTankSensor_EventGpioStatus;

            lowerTankSensor = new Sensor(6, 5);
            lowerTankSensor.Init();
            lowerTankSensor.EventDistance += LowerTankSensor_EventDistance;
            lowerTankSensor.EventGpioStatus += LowerTankSensor_EventGpioStatus;

            upperTankSensor.Ping();
            lowerTankSensor.Ping();
            await connection.InvokeAsync("SetTankLevels", upperTankHeight, lowerTankHeight);
        }
    }
}
