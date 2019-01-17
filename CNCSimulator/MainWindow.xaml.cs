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
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;
using Clifton.Core.Pipes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServerPipe serverPipe;


        public MainWindow()
        {
            InitializeComponent();
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
                }));

        }

        private void ProcessCommand(string command)
        {
            string[] splitCommand = command.Split(' ');
            switch (splitCommand[0])
            {
                case "move":
                    if(splitCommand[1] == "start")
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
                    } else
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
                default:
                    serverPipe.WriteString("Unknown command");
                    break;
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
    }
}
    