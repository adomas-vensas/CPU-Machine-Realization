﻿using MainStage.Interfaces;
using MainStage.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MainStage.Enumerators;

namespace MainStage.ViewModels;

public class VirtualMachine : IVMRegisters, INotifyPropertyChanged
{
    #region Fields

    private readonly string _virtualMachineName = "Virtual Machine";
    private readonly IResourceAllocator _resourceAllocator;

    private readonly int _blockSize = 0;

    private IReadOnlyDictionary<string, string> _instrNamesToHex = new Dictionary<string, string>()
    {
        { "LOAD",  "10" },
        { "LOADM", "11" },
        { "LOADR", "12" },
        { "LOADI", "13" },
        { "ADD",   "20" },
        { "ADDM",  "21" },
        { "ADDI",  "22" },
        { "CMP",   "30" },
        { "CMPM",  "31" },
        { "JMP",   "40" },
        { "JMPM",  "41" },
        { "JMPA",  "42" },
        { "STORE", "43" },
        { "RET",   "44" },
        { "HALT",  "00" },
    };

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
            string temp = SetValue(value, 0x1_0000);
            _r = temp.PadLeft(4, '0');
            OnPropertyChanged();
        }
    }

    private string _ic = "0000";
    public string IC
    {
        get => _ic;
        set
        {
            value.TryParseHex(out int icValue);

            if(icValue > VirtualMemory.Count)
            {
                _resourceAllocator.SetInterrupt(InterruptType.MAX_MEMORY_REACHED);
            }

            string temp = SetValue(value, 0x1_00);
            _ic = temp.PadLeft(4, '0');
            OnPropertyChanged();
        }
    }

    private string _vault = "0000";
    public string VAULT
    {
        get => _vault;
        set
        {
            _vault = value;
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

    private string _nextInput = string.Empty;
    public string NextInput
    {
        get => _nextInput;
        set
        {
            _nextInput = value?.Trim();
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

    private string _commandText = null;
    public string CommandText
    {
        get => _commandText;
        set
        {
            _commandText = value;
            _resourceAllocator.SetInterrupt(InterruptType.KEYBOARD);
            OnPropertyChanged();
        }
    }

    #endregion Properties

    #region Constructors

    public VirtualMachine(string virtualMachineName, IResourceAllocator resourceAllocator, int memorySize, int blockSize)
    {
        _virtualMachineName = virtualMachineName;
        _resourceAllocator = resourceAllocator;
        _blockSize = blockSize;
        VirtualMemory = new Memory<int, string>(memorySize, _blockSize, x => x, "00");
        VirtualMemory.MaxSizeReached += VirtualMemory_MaxSizeReached;
        VirtualMemory.MemoryOutOfRange += VirtualMemory_MemoryOutOfRange;

        DisplayMemory = GetDisplayMemory();
    }

    private void VirtualMemory_MemoryOutOfRange(object? sender, EventArgs e)
    {
        _resourceAllocator.SetInterrupt(InterruptType.INVALID_MEM_ACCESS);
    }

    private void VirtualMemory_MaxSizeReached(object? sender, EventArgs e)
    {
        _resourceAllocator.SetInterrupt(InterruptType.MAX_MEMORY_REACHED);
    }

    #endregion Constructors

    private Dictionary<string, string> GetDisplayMemory()
    {
        Dictionary<string, string> displayMemory = new();

        int i = 0;
        string key = "";
        foreach (var entry in VirtualMemory)
        {
            if(i % _blockSize == 0)
            {
                key = entry.Key.ToString("X");
                displayMemory[key] = string.Empty;
            }
            displayMemory[key] += entry.Value + " ";

            ++i;
        }

        return displayMemory;
    }

    #region Instructions

    public void Ret(int number)
    {
        IC = VAULT;
    }

    public void Store(int number)
    {
        IC.TryParseHex(out int result);

        IC = (result + 2).ToString("X");
        VAULT = IC;
    }

    public void Load(int number)
    {
        R = number.ToString("X");
    }

    public void LoadM(int memAddress)
    {
        R = VirtualMemory[memAddress];
    }

    public void LoadR(int memAddress)
    {
        R.TryParseHex(out int result);

        string firstWord = (result / 100).ToString("X").PadLeft(2, '0');
        string secondWord = (result % 100).ToString("X").PadLeft(2, '0');

        VirtualMemory[memAddress] = firstWord;
        VirtualMemory[memAddress + 1] = secondWord;
    }

    public void LoadI(int number)
    {
        IC = number.ToString("X");
    }

    public void Add(int number)
    {
        R.TryParseHex(out int rInt);

        R = (number + rInt).ToString("X");
    }

    public void AddM(int memAddress)
    {
        R.TryParseHex(out int rInt);
        VirtualMemory[memAddress].TryParseHex(out int memInt);

        R = (memInt + rInt).ToString("X");
    }

    public void AddI(int number)
    {
        R.TryParseHex(out int rInt);
        IC.TryParseHex(out int icInt);

        R = (icInt + rInt).ToString("X");
    }

    public void Cmp(int number)
    {
        R.TryParseHex(out int rInt);

        C = number != rInt;
    }

    public void CmpM(int memAddress)
    {
        R.TryParseHex(out int rInt);

        C = rInt != memAddress;
    }

    public void Jmp(int number)
    {
        IC = R;
    }

    public void JmpM(int memAddress)
    {
        if(!VirtualMemory.ContainsKey(memAddress))
        {
            _resourceAllocator.SetInterrupt(InterruptType.INVALID_MEM_ACCESS);
            return;
        }

        IC = memAddress.ToString("X");
    }

    public void JmpA(int number)
    {
        IC = number.ToString("X");
    }

    public void Halt(int number)
    {
        _resourceAllocator.Dispose(this);
    }

    #endregion Instructions

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

    public void ParseInput()
    {
        string commandString = CommandText;
        if(string.IsNullOrWhiteSpace(commandString))
        {
            _resourceAllocator.SetInterrupt(InterruptType.INVALID_INSTRUCTION);
            return;
        }

        if(!ContainsOneOrTwoWords(commandString))
        {
            return;   
        }

        commandString = commandString.ToUpper();
        string[] words = commandString.Split(' ', StringSplitOptions.TrimEntries);

        if (!_instrNamesToHex.ContainsKey(words[0]))
        {
            _resourceAllocator.SetInterrupt(InterruptType.INVALID_INSTRUCTION);
            return;
        }

        if(words.Length == 1 && (words[0] == "STORE" || words[0] == "RET" || words[0] == "JMP" || words[0] == "ADDI"))
        {
            string fw = _instrNamesToHex[words[0]];

            LoadInstructionToMemory(fw, "00");
            ExecuteCommand(words[0], 0);

            _resourceAllocator.UpdateMemory(this, fw, "00");

            return;
        }
        else if(words.Length == 1)
        {
            _resourceAllocator.SetInterrupt(InterruptType.INVALID_INSTRUCTION);
            return;
        }

        string pattern = @"^[0-9A-F]+\s*$"; //0E, 6F, 1
        Regex regex = new Regex(pattern);

        Match match = regex.Match(words[1]);

        if(match == null || !match.Success)
        {
            _resourceAllocator.SetInterrupt(InterruptType.INVALID_MEM_ACCESS);
            return;
        }

        words[1].TryParseHex(out int inputInt);

        string firstWord = _instrNamesToHex[words[0]];
        string secondWord = words[1].PadLeft(2, '0');

        LoadInstructionToMemory(firstWord, secondWord);
        ExecuteCommand(words[0], inputInt);

        _resourceAllocator.UpdateMemory(this, firstWord, secondWord);
    }


    public bool ContainsOneOrTwoWords(string input)
    {
        string pattern = @"^\b\w+\b(?:\s+\b\w+\b)?$";
        return Regex.IsMatch(input, pattern);
    }

    private void LoadInstructionToMemory(string firstWord, string secondWord)
    {
        IC.TryParseHex(out int memAddress);

        VirtualMemory[memAddress] = firstWord;
        VirtualMemory[memAddress + 1] = secondWord;

        DisplayMemory = GetDisplayMemory();
    }

    private void ExecuteCommand(string operation, int inputInt)
    {

        Action<int>? action = operation switch
        {
            "LOAD"  => Load,
            "LOADM" => LoadM,
            "LOADR" => LoadR,
            "LOADI" => LoadI,
            "ADD"   => Add,
            "ADDM"  => AddM,
            "ADDI"  => AddI,
            "CMP"   => Cmp,
            "CMPM"  => CmpM,
            "JMP"   => Jmp,
            "JMPM"  => JmpM,
            "JMPA"  => JmpA,
            "STORE" => Store,
            "RET"   => Ret,
            "HALT"  => Halt,
            _ => null
        };

        action?.Invoke(inputInt);

        IC.TryParseHex(out int icResult);
        if (!operation.Contains("JMP"))
        {
            IC = (icResult + 2).ToString("X");
        }

        DisplayMemory = GetDisplayMemory();

        _resourceAllocator.Test(this);
    }

    public void ExecuteInstructionInMemory()
    {
        IC.TryParseHex(out int icResult);

        string instruction = VirtualMemory[icResult];

        Action<int>? action = instruction switch
        {
            "LOAD" => Load,
            "LOADM" => LoadM,
            "LOADR" => LoadR,
            "LOADI" => LoadI,
            "ADD" => Add,
            "ADDM" => AddM,
            "ADDI" => AddI,
            "CMP" => Cmp,
            "CMPM" => CmpM,
            "JMP" => Jmp,
            "JMPM" => JmpM,
            "JMPA" => JmpA,
            "STORE" => Store,
            "RET" => Ret,
            "HALT" => Halt,
            _ => null
        };

        int value = int.Parse(VirtualMemory[icResult + 1]);

        action?.Invoke(value);

        if(!instruction.Contains("JMP"))
        {
            IC = (icResult + 2).ToString("X");
        }

        DisplayMemory = GetDisplayMemory();

        _resourceAllocator.Test(this);
    }

}
