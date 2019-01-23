using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using Clifton.Core.Pipes;
using System.Timers;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        ServerPipe serverPipe;
        System.Timers.Timer positionTimer;
        double x, y;
        double maxX, maxY;
        double direction = 1;
        bool receivingGCode = false;
        Thread simulate;
        List<string> gcode = new List<string>();


        public MainWindow()
        {
            InitializeComponent();
            x = 0f;
            y = 0f;
            maxX = 300;
            maxY = 300;

            serverPipe = new ServerPipe("pipe", p => p.StartStringReaderAsync());
            serverPipe.DataReceived += (sndr, args) =>
                Dispatcher.BeginInvoke(new Action(()=>
                {
                    ProcessCommand(args.String);
                }));


            serverPipe.Connected += (sndr, args) =>
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    connection.Text = "Connected";
                    positionTimer.Enabled = true;
                }));

            serverPipe.PipeClosed += (sndr, args) =>
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    connection.Text = "Disconnected";
                    positionTimer.Stop();
                }));

            // set timer to 0.1s interval
            positionTimer = new System.Timers.Timer(100);
            positionTimer.Elapsed += SendPosition;
            positionTimer.AutoReset = true;

        }

        private void ProcessCommand(string command)
        {
            

            string[] splitCommand = command.Split(' ');
            if (!receivingGCode)
            {

                switch (splitCommand[0])
                {
                    case "move":
                        if (splitCommand[1] == "start")
                        {
                            switch (splitCommand[2])
                            {
                                case "x+":
                                    xPlus.Background = Brushes.Yellow;
                                    break;
                                case "x-":
                                    xMinus.Background = Brushes.Yellow;
                                    break;
                                case "y+":
                                    yPlus.Background = Brushes.Yellow;
                                    break;
                                case "y-":
                                    yMinus.Background = Brushes.Yellow;
                                    break;
                            }
                        }
                        else
                        {
                            switch (splitCommand[2])
                            {
                                case "x+":
                                    xPlus.Background = Brushes.Gray;
                                    break;
                                case "x-":
                                    xMinus.Background = Brushes.Gray;
                                    break;
                                case "y+":
                                    yPlus.Background = Brushes.Gray;
                                    break;
                                case "y-":
                                    yMinus.Background = Brushes.Gray;
                                    break;
                            }
                        }
                        break;

                    case "M03":
                        PlasmaOn();
                        break;

                    case "M05":
                        PlasmaOff();
                        break;

                    case "GCODE":
                        receivingGCode = true;
                        break;

                    default:
                        serverPipe.WriteString("Unknown command");
                        break;
                }
            }
            else
            {
                if (command == "END")
                {
                    Debug.WriteLine(command);

                    receivingGCode = false;
                    simulate = new Thread(simulateGCode);
                    simulate.Start();
                }
                else
                {
                    gcode.Add(command);
                }
            }
        }

        private void Napaka1_Click(object sender, RoutedEventArgs e)
        {
            serverPipe.WriteString("ERROR 1");
        }

        private void Napaka2_Click(object sender, RoutedEventArgs e)
        {
            serverPipe.WriteString("ERROR 2");
        }

        private void Napaka3_Click(object sender, RoutedEventArgs e)
        {
            serverPipe.WriteString("ERROR 3");
        }

        private void SendPosition(Object source, ElapsedEventArgs e)
        {
            serverPipe.WriteString(String.Format("POSITION {0} {1}", x, y));
            if (x >= maxX && y >= maxY)
                direction = -direction;
            x += direction;
            y += direction;
            this.Dispatcher.Invoke(() =>
            {
                posX.Text = "X: " + x;
                posY.Text = "Y: " + y;
            });
        }

        private void PlasmaOn()
        {
            plasma.Background = Brushes.Yellow;
            serverPipe.WriteString("PLASMA ON");
        }

        private void PlasmaOff()
        {
            plasma.Background = Brushes.Gray;
            serverPipe.WriteString("PLASMA OFF");
        }

        private void simulateGCode()
        {
            foreach(string line in gcode){
                serverPipe.WriteString("DONE " + line.Split(':')[0]);
                Thread.Sleep(1000);
            }
        }
    }
}
    