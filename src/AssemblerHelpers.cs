using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAssembler
{
    /// <summary>
    /// Contains generic assembler helper methods.
    /// </summary>
    public static class AssemblerHelpers
    {
        /// <summary>
        /// Indicates if the given value fits in a byte
        /// </summary>
        /// <param name="value">The value</param>
        public static bool IsValidByteValue(int value)
        {
            return value >= -128 && value < 128;
        }
    }
}
