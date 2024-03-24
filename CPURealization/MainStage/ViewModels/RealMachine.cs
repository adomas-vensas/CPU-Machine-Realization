using MainStage.Enumerators;
using MainStage.Interfaces;
using MainStage.Shared;
using MainStage.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
    private Dictionary<VirtualMachine, Dictionary<int, int>> _vmMemoryBlocks = new(); //In what to what Part of real Memory will the VM exist
    private HashSet<int> _freeRealMemoryBlocks = null;

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

    #endregion Properties

    #region Constructors

    public RealMachine()
    {
        _freeRealMemoryBlocks = Enumerable.Range(0, MAX_SIZE).Select(x => x).ToHashSet();
        DisplayMemory = GetDisplayMemory(BLOCK_SIZE, MAX_SIZE);
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

    public void Test(VirtualMachine machine)
    {
        IR.TryParseHex(out int irResult);

        if(irResult > 0)
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

    #endregion Methods

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  
    }

    
}
