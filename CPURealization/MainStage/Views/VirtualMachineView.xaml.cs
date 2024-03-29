﻿using MainStage.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace MainStage.Views
{
    /// <summary>
    /// Interaction logic for VirtualMachineView.xaml
    /// </summary>
    public partial class VirtualMachineView : Window
    {
        public VirtualMachineView()
        {
            InitializeComponent();
        }

        private void OnReturnPressed(object sender, KeyEventArgs e)
        {
            if(e.Key != Key.Return)
            {
                return;
            }

            var vm = DataContext as VirtualMachine;

            if (vm == null)
            {
                return;
            }

            vm.InputHistory.Add(vm.CommandText);

            vm.ParseInput();

            vm.CommandText = string.Empty;
        }

        private void OnExecuteNext(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as VirtualMachine;

            if(vm == null)
            {
                return;
            }

            vm.ExecuteInstructionInMemory();

        }
    }
}
