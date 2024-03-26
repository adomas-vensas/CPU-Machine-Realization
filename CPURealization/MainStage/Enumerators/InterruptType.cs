using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Enumerators;

public enum InterruptType
{
    HALT = 0x01,
    INVALID_MEM_ACCESS = 0x02,
    INVALID_INSTRUCTION = 0x03,
    KEYBOARD = 0x04,
    SYSTEM_CALL = 0x05,
    MAX_MEMORY_REACHED = 0x06,
    SUPER_MODE = 0x07,
    USER_MODE = 0x08,
}
