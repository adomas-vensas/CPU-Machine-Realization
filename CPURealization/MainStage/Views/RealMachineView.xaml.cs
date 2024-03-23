﻿using MainStage.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainStage.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RealMachineView : Window
    {
        public RealMachineView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as RealMachine;

            if(vm == null)
            {
                return;
            }

            VirtualMachine virtualMachine = vm.CreateVirtualMachine();

            VirtualMachineView virtualMachineView = new VirtualMachineView();
            virtualMachineView.Closing += (sender, e) => virtualMachine.Halt();
            virtualMachineView.DataContext = virtualMachine;
            virtualMachineView.Title = virtualMachine.VirtualMachineName;
            virtualMachineView.Show();
        }
    }
}