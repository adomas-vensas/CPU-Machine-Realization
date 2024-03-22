using MainStage.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Interfaces;

public interface IRMRegisters : IVMRegisters
{
    public string PTR { get; set; }
    public ModeType MODE { get; set; }
    public string PI { get; set; } 
    public string SI { get; set; } 
}
