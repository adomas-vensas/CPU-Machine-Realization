using MainStage.Enumerators;
using MainStage.Interfaces;
using MainStage.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MainStage;

public class RealMachine : IRMRegisters, IResourceAllocator, INotifyPropertyChanged
{
    #region Fields

    public const int BLOCK_SIZE = 10;
    public const int MAX_SIZE = BLOCK_SIZE * 50;

    private List<VirtualMachine> _virtualMachines = new List<VirtualMachine>();
    
    private Memory<int, string> _realMemory = new Memory<int, string>(MAX_SIZE, BLOCK_SIZE, x => x, "0000");
    private Dictionary<VirtualMachine, List<int>> _vmMemoryBlocks = new(); //In what to what Part of real Memory will the VM exist
    private HashSet<int> _freeRealMemoryBlocks = null;

    #endregion Fields

    #region Properties

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

    private string _pi = "0";
    public string PI
    {
        get => _pi;
        set
        {
            _pi = SetValue(value, 0x10);
            OnPropertyChanged();
        }
    }

    private string _si = "0";
    public string SI
    {
        get => _si;
        set
        {
            _si = SetValue(value, 0x10);
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
    }

    #endregion Constructors

    #region Methods

    public void CreateVirtualMachine()
    {
        VirtualMachine virtualMachine = new VirtualMachine(this, 10);

        _virtualMachines.Add(virtualMachine);

        int memoryBlockId = (int) Random.Shared.NextInt64(0, _freeRealMemoryBlocks.Count + 1);
        _freeRealMemoryBlocks.Remove(memoryBlockId);

        _vmMemoryBlocks[virtualMachine] = new List<int>() { memoryBlockId };
    }

    private void UpdateRealMemory(VirtualMachine virtualMachine, int vmMemAddress)
    {
        int x = vmMemAddress / 10;
        int y = vmMemAddress % 10;

    }

    public void Test(VirtualMachine machine)
    {
        throw new NotImplementedException();
    }

    public void ProvideMemory(VirtualMachine machine)
    {
        throw new NotImplementedException();

    }

    public void Dispose(VirtualMachine machine)
    {
        throw new NotImplementedException();
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
