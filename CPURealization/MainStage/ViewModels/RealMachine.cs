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

    public const int BLOCK_SIZE = 10;
    public const int MAX_SIZE = BLOCK_SIZE * 50;

    private List<VirtualMachine> _virtualMachines = new List<VirtualMachine>();
    
    private Memory<int, string> _realMemory = new Memory<int, string>(MAX_SIZE, BLOCK_SIZE, x => x, "0000");
    private readonly Dictionary<int, Action<int>> _interruptTable;
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

    private string _ir = "0000";
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

    private string _ic = "00";
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
        DisplayMemory = GetDisplayMemory(BLOCK_SIZE, MAX_SIZE);

        _os = new OperatingSystem();

        _interruptTable = new Dictionary<int, Action<int>>
        {
            { 1, new Action<int>(_os.GiveControl) },
            { 2, new Action<int>(_os.GiveControl) },
            { 3, new Action<int>(_os.GiveControl) },
            { 4, new Action<int>(_os.GiveControl) },
            { 5, new Action<int>(_os.GiveControl) },
            { 6, new Action<int>(_os.GiveControl) },
            { 7, new Action<int>(_os.GiveControl) },
            { 8, new Action<int>(_os.GiveControl) }
        };
    }

    #endregion Constructors

    #region Methods

    private Dictionary<string, string> GetDisplayMemory(int blockSize, int maxSize)
    {
        Dictionary<string, string> displayMemory = new();

        for(int i = 0; i <= maxSize; i += blockSize)
        {
            displayMemory[i.ToString("X")] = Enumerable.Repeat("0000", blockSize).Aggregate((x, y) => x + " " + y);
        }

        return displayMemory;
    }

    public VirtualMachine CreateVirtualMachine()
    {
        string virtualMachineName = $"Virtual Machine {_virtualMachines.Count + 1}";
        VirtualMachine virtualMachine = new VirtualMachine(virtualMachineName,this, 10);

        _virtualMachines.Add(virtualMachine);

        int memoryBlockId = (int) Random.Shared.NextInt64(0, _freeRealMemoryBlocks.Count + 1);
        _freeRealMemoryBlocks.Remove(memoryBlockId);

        _vmMemoryBlocks[virtualMachine] = new Dictionary<int ,int>() { { 0, memoryBlockId } };

        return virtualMachine;
    }

    private void UpdateRealMemory(VirtualMachine virtualMachine, int instructionBytes)
    {
        string bytes = instructionBytes.ToString("X");

        string opCodeHex = bytes.Substring(0, 1);
        string memAddressHex = bytes.Substring(opCodeHex.Length);

        memAddressHex.TryParseHex(out int memAddress);
        opCodeHex.TryParseHex(out int opCode);

        int x = memAddress / 10;
        int y = memAddress % 10;

        int realMemoryId = _vmMemoryBlocks[virtualMachine][x];

        _realMemory[realMemoryId + y] = opCodeHex;
        _realMemory[realMemoryId + y + 1] = memAddressHex;
    }

    public void Add(int x, int y)
    {
        int address = GetMemAddress(x, y);
        string memValueStr = _realMemory[address];

        memValueStr.TryParseHex(out int memValue);
        R.TryParseHex(out int rValue);

        int result = (memValue + rValue);

        if (result > 0x1_0000)
        {
            result %= 0x1_0000;
        }

        R = result.ToString("X");

        Test(this);
    }

    public void Count(int x, int y)
    {
        if (C)
        {
            int address = GetMemAddress(x, y);
            IC = _realMemory[address];
        }

        Test(this);
    }

    public void Flip(int x, int y)
    {
        int address = GetMemAddress(x, y);

        C = (R == _realMemory[address]);

        Test(this);
    }

    public void Go(int x, int y)
    {
        IC = GetMemAddress(x, y).ToString("X");

        Test(this);
    }

    public void Load(int x, int y)
    {
        int address = GetMemAddress(x, y);
        string memValue = _realMemory[address];

        R = memValue;

        Test(this);
    }

    public void Not(int x, int y)
    {
        if (!C)
        {
            IC = GetMemAddress(x, y).ToString("X");
        }

        Test(this);
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

        _realMemory[address] = R;

        Test(this);
    }

    public void Test(VirtualMachine machine)
    {
        IR.TryParseHex(out int irResult);

        if(irResult > 0)
        {
            Console.WriteLine("Interrupt detected");

            _realMemory.InterruptTable[irResult].Invoke(irResult);
        }
    }

    public void Test(RealMachine machine)
    {
        IR.TryParseHex(out int irResult);

        if (irResult > 0)
        {
            Console.WriteLine("Interrupt detected");

            _realMemory.InterruptTable[irResult].Invoke(irResult);
        }
    }

    public void ProvideMemory(VirtualMachine machine)
    {
        throw new NotImplementedException();
    }

    public void Dispose(VirtualMachine machine)
    {
        _virtualMachines.Remove(machine);
    }

    public void SetInterrupt(int interruptCode)
    {
        IR = interruptCode.ToString("X");
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

        ExecuteCommand(parts[0], x, y);
    }

    private int GetMemAddress(int x, int y)
    {
        return x * MAX_SIZE + y;
    }

    private (int x, int y) GetMemAdress(string memAddressStr)
    {
        int.TryParse(memAddressStr, out int memAddress);

        return (memAddress / MAX_SIZE, memAddress % MAX_SIZE);
    }

    private void ExecuteCommand(string operation, int x, int y)
    {
        switch (operation)
        {
            case "LOAD":
                Load(x, y);
                break;
            case "ADD":
                Add(x, y);
                break;
            case "STORE":
                Store(x, y);
                break;
            case "FLIP":
                Flip(x, y);
                break;
            case "COUNT":
                Count(x, y);
                break;
            case "READ":
                Read(x);
                break;
            case "SEND":
                Send(x);
                break;
            case "NOT":
                Not(x, y);
                break;
            case "GO":
                Go(x, y);
                break;

            default:
                Test(this);
                return;
        }


    }

    #endregion Methods

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  
    }

    
}
