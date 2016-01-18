using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAssembler.x64
{
    /// <summary>
    /// The base int registers
    /// </summary>
    public enum Register : byte
    {
        AX = 0,
        CX = 1,
        DX = 2,
        BX = 3,
        SP = 4,
        BP = 5,
        SI = 6,
        DI = 7,
    }

    /// <summary>
    /// The extended int registers
    /// </summary>
    public enum ExtendedRegister : byte
    {
        R8 = 0,
        R9 = 1,
        R10 = 2,
        R11 = 3,
        R12 = 4,
        R13 = 5,
        R14 = 6,
        R15 = 7
    }

    /// <summary>
    /// The float registers
    /// </summary>
    public enum FloatRegister : byte
    {
        XMM0 = 0,
        XMM1 = 1,
        XMM2 = 2,
        XMM3 = 3,
        XMM4 = 4,
        XMM5 = 5,
        XMM6 = 6,
        XMM7 = 7,
    }

    /// <summary>
    /// Represents an integer register
    /// </summary>
    public struct IntRegister
    {
        /// <summary>
        /// Indicates if the register is a base register
        /// </summary>
        public bool IsBase { get; }

        /// <summary>
        /// Returns the base register
        /// </summary>
        public Register BaseRegister { get; }

        /// <summary>
        /// Returns the extended register
        /// </summary>
        public ExtendedRegister ExtendedRegister { get; }

        /// <summary>
        /// Creates a new base register
        /// </summary>
        /// <param name="baseRegister">The base register</param>
        public IntRegister(Register baseRegister)
        {
            this.IsBase = true;
            this.BaseRegister = baseRegister;
            this.ExtendedRegister = ExtendedRegister.R8;
        }

        /// <summary>
        /// Creates a new extended register
        /// </summary>
        /// <param name="extendedRegister">The extended register</param>
        public IntRegister(ExtendedRegister extendedRegister)
        {
            this.IsBase = false;
            this.BaseRegister = Register.AX;
            this.ExtendedRegister = extendedRegister;
        }

        public override string ToString()
        {
            if (this.IsBase)
            {
                return "R" + this.BaseRegister;
            }
            else
            {
                return this.ExtendedRegister.ToString();
            }
        }

        /// <summary>
        /// Implicits converts the given base register into an int register
        /// </summary>
        /// <param name="baseRegister">The register</param>
        public static implicit operator IntRegister(Register baseRegister)
        {
            return new IntRegister(baseRegister);
        }

        /// <summary>
        /// Implicits converts the given extended register into an int register
        /// </summary>
        /// <param name="extendedRegister">The register</param>
        public static implicit operator IntRegister(ExtendedRegister extendedRegister)
        {
            return new IntRegister(extendedRegister);
        }

        /// <summary>
        /// Checks if lhs == rhs
        /// </summary>
        /// <param name="lhs">The left hand side</param>
        /// <param name="rhs">The right hand side</param>
        public static bool operator ==(IntRegister lhs, IntRegister rhs)
        {
            if (lhs.IsBase != rhs.IsBase)
            {
                return false;
            }

            if (lhs.IsBase)
            {
                return lhs.BaseRegister == rhs.BaseRegister;
            }
            else
            {
                return lhs.ExtendedRegister == rhs.ExtendedRegister;
            }
        }

        /// <summary>
        /// Checks if lhs != rhs
        /// </summary>
        /// <param name="lhs">The left hand side</param>
        /// <param name="rhs">The right hand side</param>
        public static bool operator !=(IntRegister lhs, IntRegister rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Checks if the current object is equal to the given
        /// </summary>
        /// <param name="obj">The object</param>
        public override bool Equals(object obj)
        {
            if (!(obj is IntRegister))
            {
                return false;
            }

            var other = (IntRegister)obj;
            return this == other;
        }

        /// <summary>
        /// Computes the hash code
        /// </summary>
        public override int GetHashCode()
        {
            if (this.IsBase)
            {
                return this.IsBase.GetHashCode() + 31 * (int)this.BaseRegister;
            }
            else
            {
                return this.IsBase.GetHashCode() + 31 * (int)this.ExtendedRegister;
            }
        }
    }

    /// <summary>
    /// The types of the hardware registers
    /// </summary>
    public enum HardwareRegisterType
    {
        Int,
        Float
    }

    /// <summary>
    /// Represents a hardware register
    /// </summary>
    public struct HardwareRegister
    {
        /// <summary>
        /// The type of the register
        /// </summary>
        public HardwareRegisterType Type { get; }

        /// <summary>
        /// The int register
        /// </summary>
        public IntRegister IntRegister { get; }

        /// <summary>
        /// The float register
        /// </summary>
        public FloatRegister FloatRegister { get; }

        /// <summary>
        /// Creates a new int register
        /// </summary>
        /// <param name="intRegister">The int register</param>
        public HardwareRegister(IntRegister intRegister)
        {
            this.IntRegister = intRegister;
            this.Type = HardwareRegisterType.Int;
            this.FloatRegister = FloatRegister.XMM0;
        }

        /// <summary>
        /// Creates a new float register
        /// </summary>
        /// <param name="floatRegister">The float register</param>
        public HardwareRegister(FloatRegister floatRegister)
        {
            this.IntRegister = Register.AX;
            this.Type = HardwareRegisterType.Float;
            this.FloatRegister = floatRegister;
        }

        /// <summary>
        /// Returns a string representation of the register
        /// </summary>
        public override string ToString()
        {
            switch (this.Type)
            {
                case HardwareRegisterType.Int:
                    return this.IntRegister.ToString();
                case HardwareRegisterType.Float:
                    return this.FloatRegister.ToString();
                default:
                    return "";
            }
        }

        /// <summary>
        /// Implicits converts the given int register into a hardware register
        /// </summary>
        /// <param name="register">The register</param>
        public static implicit operator HardwareRegister(IntRegister register)
        {
            return new HardwareRegister(register);
        }

        /// <summary>
        /// Implicits converts the given float register into a hardware register
        /// </summary>
        /// <param name="floatRegister">The register</param>
        public static implicit operator HardwareRegister(FloatRegister floatRegister)
        {
            return new HardwareRegister(floatRegister);
        }

        /// <summary>
        /// Checks if lhs == rhs
        /// </summary>
        /// <param name="lhs">The left hand side</param>
        /// <param name="rhs">The right hand side</param>
        public static bool operator ==(HardwareRegister lhs, HardwareRegister rhs)
        {
            if (lhs.Type != rhs.Type)
            {
                return false;
            }

            switch (lhs.Type)
            {
                case HardwareRegisterType.Int:
                    return lhs.IntRegister == rhs.IntRegister;
                case HardwareRegisterType.Float:
                    return lhs.FloatRegister == rhs.FloatRegister;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if lhs != rhs
        /// </summary>
        /// <param name="lhs">The left hand side</param>
        /// <param name="rhs">The right hand side</param>
        public static bool operator !=(HardwareRegister lhs, HardwareRegister rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Checks if the current object is equal to the given
        /// </summary>
        /// <param name="obj">The object</param>
        public override bool Equals(object obj)
        {
            if (!(obj is HardwareRegister))
            {
                return false;
            }

            var other = (HardwareRegister)obj;

            if (this.Type != other.Type)
            {
                return false;
            }

            switch (this.Type)
            {
                case HardwareRegisterType.Int:
                    return this.IntRegister == other.IntRegister;
                case HardwareRegisterType.Float:
                    return this.FloatRegister == other.FloatRegister;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Computes the hash code
        /// </summary>
        public override int GetHashCode()
        {
            switch (this.Type)
            {
                case HardwareRegisterType.Int:
                    return this.Type.GetHashCode() + 31 * this.IntRegister.GetHashCode();
                case HardwareRegisterType.Float:
                    return this.Type.GetHashCode() + 31 * this.FloatRegister.GetHashCode();
                default:
                    return 0;
            }
        }
    }
}
