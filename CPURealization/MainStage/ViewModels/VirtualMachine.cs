﻿using MainStage.Interfaces;
using MainStage.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MainStage.ViewModels;

public class VirtualMachine : IInstructions, IVMRegisters, INotifyPropertyChanged
{
    #region Fields

    private readonly string _virtualMachineName = "Virtual Machine";
    private readonly IResourceAllocator _resourceAllocator;
    private const int BLOCK_SIZE = 10;


    #endregion Fields

    #region Properties

    public string VirtualMachineName => _virtualMachineName;

    private Dictionary<string, string> _displayMemory = null;
    public Dictionary<string, string> DisplayMemory
    {
        get => _displayMemory;
        set
        {
            _displayMemory = value;
            OnPropertyChanged();
        }
    }

    protected Memory<int, string> VirtualMemory { get; set; }

    private string _r = "0000";
    public string R
    {
        get => _r;
        set
        {
            _r = SetValue(value, 0x1_0000);
            OnPropertyChanged();
        }
    }

    private string _ic = "0000";
    public string IC
    {
        get => _ic;
        set
        {
            _ic = SetValue(value, 0x1_00);
            OnPropertyChanged();
        }
    }

    private bool _c = false;
    public bool C
    {
        get => _c;
        set
        {
            _c = value;
            OnPropertyChanged();
        }
    }

    private string _nextInput = "";
    public string NextInput
    {
        get => _nextInput;
        set
        {
            _nextInput = value;
            OnPropertyChanged();
        }
    }

    

    //private RelayCommand? _returnPressedCommand;
    //public RelayCommand ReturnPressedCommand
    //{
    //    get { return _returnPressedCommand ?? (_returnPressedCommand = new RelayCommand(ReturnPressedCommandHandler, param => true)); }
    //}

    #endregion Properties

    #region Constructors

    public VirtualMachine(string virtualMachineName, IResourceAllocator resourceAllocator, int memorySize)
    {
        _virtualMachineName = virtualMachineName;
        _resourceAllocator = resourceAllocator;
        VirtualMemory = new Memory<int, string>(memorySize, BLOCK_SIZE, x => x, "0000");

        DisplayMemory = GetDisplayMemory(BLOCK_SIZE, memorySize);
    }

    #endregion Constructors

    private Dictionary<string, string> GetDisplayMemory(int blockSize, int maxSize)
    {
        Dictionary<string, string> displayMemory = new();

        for (int i = 0; i <= maxSize; i += blockSize)
        {
            displayMemory[i.ToString("X")] = Enumerable.Repeat("0000", blockSize).Aggregate((x, y) => x + " " + y);
        }

        return displayMemory;
    }

    public void Add(int x, int y)
    {
        int address = GetMemAddress(x, y);
        string memValueStr = VirtualMemory[address];

        memValueStr.TryParseHex(out int memValue);
        R.TryParseHex(out int rValue);

        int result = (memValue + rValue);

        if(result > 0x1_0000)
        {
            result %= 0x1_0000;
        }

        R = result.ToString("X");

        _resourceAllocator.Test(this);
    }

    public void Count(int x, int y)
    {
        if(C)
        {
            int address = GetMemAddress(x, y);
            IC = VirtualMemory[address];
        }

        _resourceAllocator.Test(this);
    }

    public void Flip(int x, int y)
    {
        int address = GetMemAddress(x, y);

        C = (R == VirtualMemory[address]);

        _resourceAllocator.Test(this);
    }

    public void Go(int x, int y)
    {
        IC = GetMemAddress(x, y).ToString("X");

        _resourceAllocator.Test(this);
    }

    public void Halt()
    {
        _resourceAllocator.Dispose(this);
    }

    public void Load(int x, int y)
    {
        int address = GetMemAddress(x, y);
        string memValue = VirtualMemory[address];

        R = memValue;

        _resourceAllocator.Test(this);
    }

    public void Not(int x, int y)
    {
        if(!C)
        {
            IC = GetMemAddress(x, y).ToString("X");
        }

        _resourceAllocator.Test(this);
    }

    public void Read(int x)
    {
        throw new NotImplementedException();
    }

    public void Send(int x)
    {
        throw new NotImplementedException();
    }

    public void Store(int x, int y)
    {
        int address = GetMemAddress(x, y);

        VirtualMemory[address] = R;

        _resourceAllocator.Test(this);
    }

    private int GetMemAddress(int x, int y)
    {
        return x * BLOCK_SIZE + y;
    }
    private string SetValue(string hexString, int modValue)
    {
        hexString.TryParseHex(out int result);
        result %= modValue;

        return result.ToString("X");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    
}
