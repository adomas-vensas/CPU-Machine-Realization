using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStage.Shared;

public static class Extensions
{
    public static bool TryParseHex(this string hexString, out int result)
    {
        return int.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, null, out result);
    }
}
