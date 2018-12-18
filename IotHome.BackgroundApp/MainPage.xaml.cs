using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IotHome.BackgroundApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HubConnection connection;
        private bool motorState = false;
        private bool boosterState = false;

        public MainPage()
        {
            this.InitializeComponent();

            connection = new HubConnectionBuilder()
                .WithUrl("http://192.168.1.12:5000/app")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }

        private async void MotorState_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                motorState = !motorState;

                await connection.InvokeAsync("Controller",
                    motorState, boosterState);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                txtBox.Text = ex.Message;
            }
        }

        private async void BoosterState_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                boosterState = !boosterState;

                await connection.InvokeAsync("Controller",
                    motorState, boosterState);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                txtBox.Text = ex.Message;
            }
        }

        private async void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            Connect();

            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                txtBox.Text = ex.Message;
            }
        }

        private void Connect()
        {
            connection.On<bool, bool>("Status", (motorstate, boosterstate) =>
            {
                motorState = motorstate;
                boosterState = boosterstate;

                MotorState_TextBlock.Text = motorState.ToString();
                BoosterState_TextBlock.Text = boosterState.ToString();

                Motor_Toggle.Toggled += MotorState_Toggled;
                Booster_Toggle.Toggled += BoosterState_Toggled;
            });
        }
    }
}
