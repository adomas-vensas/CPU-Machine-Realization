using MainStage.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Interfaces;

public interface IRMRegisters : IVMRegisters
{
    public int PTR { get; set; }
    public ModeType MODE { get; set; }
    public int PI { get; set; } 
    public int SI { get; set; } 
}
