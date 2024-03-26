using MainStage.Enumerators;
using MainStage.Interfaces;
using MainStage.Shared;
using MainStage.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.XPath;

namespace MainStage.ViewModels;

public class RealMachine : IRMRegisters, IResourceAllocator, INotifyPropertyChanged
{
    #region Fields

    public const int BLOCK_SIZE = 0x0A;
    public const int MAX_SIZE = BLOCK_SIZE * 0x32; // 10 * 50

    private List<VirtualMachine> _virtualMachines = new List<VirtualMachine>();
    
    private Memory<int, string> _realMemory = new Memory<int, string>(MAX_SIZE, BLOCK_SIZE, x => x, "00");
    private Dictionary<VirtualMachine, Dictionary<int, int>> _vmMemoryBlocks = new(); //In what to what Part of real Memory will the VM exist
    private HashSet<int> _freeRealMemoryBlocks = null;
    private readonly IOS _os;

    #endregion Fields

    #region Properties

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

    private string _ptr = "0000";
    public string PTR
    {
        get => _ptr;
        set
        {
            _ptr = SetValue(value, 0x1_0000);
            OnPropertyChanged();
        }
    }

    public ModeType MODE { get; set; }

    private string _ir = "00";
    public string IR
    {
        get => _ir;
        set
        {
            _ir = SetValue(value, 0x10);
            OnPropertyChanged();
        }
    }

    private string _r = "0000";
    public string R
    {
        get => _r;
        set
        {
            _r = SetValue(value, 1_0000);
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

    private ObservableCollection<string> _inputHistory = new ObservableCollection<string>();
    public ObservableCollection<string> InputHistory
    {
        get => _inputHistory;
        set
        {
            _inputHistory = value;
            OnPropertyChanged();
        }
    }

    #endregion Properties

    #region Constructors

    public RealMachine()
    {
        _freeRealMemoryBlocks = Enumerable.Range(0, MAX_SIZE).Select(x => x).ToHashSet();
        DisplayMemory = GetDisplayMemory();

        _os = new OperatingSystem();

        
    }

    #endregion Constructors

    #region Methods

    private Dictionary<string, string> GetDisplayMemory()
    {
        Dictionary<string, string> displayMemory = new();

        int i = 0;
        string key = "";
        foreach (var entry in _realMemory)
        {
            if (i % BLOCK_SIZE == 0)
            {
                key = entry.Key.ToString("X");
                displayMemory[key] = string.Empty;
            }
            displayMemory[key] += entry.Value + " ";

            ++i;
        }

        return displayMemory;
    }

    public VirtualMachine CreateVirtualMachine()
    {
        int memorySize =  (int) (MAX_SIZE * 0.25);
        string virtualMachineName = $"Virtual Machine {_virtualMachines.Count + 1}";
        VirtualMachine virtualMachine = new VirtualMachine(virtualMachineName,this, memorySize, BLOCK_SIZE);

        _virtualMachines.Add(virtualMachine);

        _vmMemoryBlocks[virtualMachine] = new Dictionary<int, int>();

        int count = (int) Math.Ceiling((double)memorySize / BLOCK_SIZE);

        for (int i = 0; i < count; ++i)
        {
            int memoryBlockId = (int)Random.Shared.NextInt64(0, _freeRealMemoryBlocks.Count + 1);
            _freeRealMemoryBlocks.Remove(memoryBlockId);
            _vmMemoryBlocks[virtualMachine].Add(i, memoryBlockId);
        }

        return virtualMachine;
    }

    public void UpdateMemory(VirtualMachine machine, string firstWord, string secondWord)
    {
        machine.IC.TryParseHex(out int icValue);

        icValue -= 2;

        int x = icValue / BLOCK_SIZE;
        int y = icValue % BLOCK_SIZE;

        int realMemoryId = _vmMemoryBlocks[machine][x];

        _realMemory[realMemoryId + y] = firstWord;
        _realMemory[realMemoryId + y + 1] = secondWord;

        DisplayMemory = GetDisplayMemory();

        IC = (realMemoryId + y).ToString("X");
    }

    public void Test(VirtualMachine machine)
    {
        IR.TryParseHex(out int irResult);

        if(irResult > 0)
        {
            Console.WriteLine("Interrupt detected");

            if(_realMemory != null && _realMemory.InterruptTable != null)
            {
                _realMemory?.InterruptTable[irResult]?.Invoke(irResult);
            }

        }
    }

    public void Test(RealMachine machine)
    {
        IR.TryParseHex(out int irResult);

        if (irResult > 0)
        {
            Console.WriteLine("Interrupt detected");

            if (_realMemory != null && _realMemory.InterruptTable != null)
            {
                _realMemory?.InterruptTable[irResult]?.Invoke(irResult);
            }
        }
    }

    public void ProvideMemory(VirtualMachine machine)
    {
        
    }

    public void Dispose(VirtualMachine machine)
    {
        _virtualMachines.Remove(machine);
    }

    public void SetInterrupt(InterruptType interrupt)
    {
        IR = interrupt.ToString("X");
    }

    private string SetValue(string hexString, int modValue)
    {
        hexString.TryParseHex(out int result);
        result %= modValue;

        return result.ToString("X");
    }

    public void ParseInput(string commandString)
    {
        if (string.IsNullOrWhiteSpace(commandString))
        {
            Test(this);
            return;
        }

        commandString = commandString.ToUpper();
        string pattern = @"^\s*[A-Z]{1,5} [0-9A-F]+\s*$"; //HALT 0E, LOAD 6F, HELLO 1
        Regex regex = new Regex(pattern);

        Match match = regex.Match(commandString);

        if (match == null || !match.Success)
        {
            Test(this);
            return;
        }

        string instructionStr = match.Value.Trim();
        string[] parts = instructionStr.Split(' ');

        (int x, int y) = GetMemAdress(parts[1]);

        //ExecuteCommand(parts[0], x, y);
    }

    private int GetMemAddress(int x, int y)
    {
        return x * BLOCK_SIZE + y;
    }

    private (int x, int y) GetMemAdress(string memAddressStr)
    {
        int.TryParse(memAddressStr, out int memAddress);

        return (memAddress / BLOCK_SIZE, memAddress % BLOCK_SIZE);
    }

    #endregion Methods

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  
    }

    public void DisposeMemory(VirtualMachine machine)
    {
        
    }
}
