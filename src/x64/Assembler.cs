using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAssembler.x64
{
    /// <summary>
    /// Represents a memory operand
    /// </summary>
    public struct MemoryOperand
    {
        /// <summary>
        /// The register where the address is stored
        /// </summary>
        public IntRegister Register { get; }

        /// <summary>
        /// Indicates if the operand has an offset
        /// </summary>
        public bool HasOffset { get; }

        /// <summary>
        /// The offset
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Creates a new memory operand
        /// </summary>
        /// <param name="register">The register where the address is stored</param>
        public MemoryOperand(IntRegister register)
        {
            this.Register = register;
            this.HasOffset = false;
            this.Offset = 0;
        }

        /// <summary>
        /// Creates a new memory operand with an offset
        /// </summary>
        /// <param name="register">The register where the address is stored</param>
        /// <param name="offset">The offset</param>
        public MemoryOperand(IntRegister register, int offset)
        {
            this.Register = register;
            this.HasOffset = true;
            this.Offset = offset;
        }

        public override string ToString()
        {
            if (this.HasOffset)
            {
                if (this.Offset > 0)
                {
                    return $"[{this.Register}+{this.Offset}]";
                }
                else
                {
                    return $"[{this.Register}{this.Offset}]";
                }
            }
            else
            {
                return $"[{this.Register}]";
            }
        }
    }

    /// <summary>
    /// The jump condition
    /// </summary>
    public enum JumpCondition
    {
        Always,
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }

    /// <summary>
    /// The data sizes for the memory instructions
    /// </summary>
    public enum DataSize
    {
        Size8,
		Size16,
		Size32,
		Size64,
	};

    /// <summary>
    /// Represents an assembler
    /// </summary>
    public class Assembler
    {
        private readonly IList<byte> generatedCode;

        /// <summary>
        /// The default memory data size
        /// </summary>
        public const DataSize DefaultMemoryDataSize = DataSize.Size64;

        /// <summary>
        /// Creates a new assembler
        /// </summary>
        /// <param name="generatedCode">The assembler</param>
        public Assembler(IList<byte> generatedCode)
        {
            this.generatedCode = generatedCode;
        }

        /// <summary>
        /// Returns the generated code
        /// </summary>
        public IList<byte> GeneratedCode => this.generatedCode;

        /// <summary>
        /// The size of a register
        /// </summary>
        public const int RegisterSize = RawAssembler.RegisterSize;

        /// <summary>
        /// Generates code for a two register operand instruction
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        /// <param name="op1">The first operand</param>
        /// <param name="op2">The second operand</param>
        /// <param name="inst1">The first instruction</param>
        /// <param name="inst2"> The second instruction</param> 
        /// <param name="inst3">The third instruction</param>
        /// <param name="inst4"> The fourth instruction</param> 
        private static void GenerateTwoRegistersInstruction(IList<byte> generatedCode, IntRegister op1, IntRegister op2,
            Action<IList<byte>, Register, Register> inst1, Action<IList<byte>, ExtendedRegister, ExtendedRegister> inst2,
            Action<IList<byte>, Register, ExtendedRegister> inst3, Action<IList<byte>, ExtendedRegister, Register> inst4)
        {
            if (op1.IsBase && op2.IsBase)
            {
                inst1(generatedCode, op1.BaseRegister, op2.BaseRegister);
            }
            else if (!op1.IsBase && !op2.IsBase)
            {
                inst2(generatedCode, op1.ExtendedRegister, op2.ExtendedRegister);
            }
            else if (op1.IsBase && !op2.IsBase)
            {
                inst3(generatedCode, op1.BaseRegister, op2.ExtendedRegister);
            }
            else
            {
                inst4(generatedCode, op1.ExtendedRegister, op2.BaseRegister);
            }
        }

        /// <summary>
        /// Generates code for an one register operand instruction
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        /// <param name="op">The operand</param>
        /// <param name="inst1">The first instruction</param>
        /// <param name = "inst2" > The second instruction</param> 
        private static void GenerateOneRegisterInstruction(IList<byte> generatedCode, IntRegister op,
            Action<IList<byte>, Register> inst1, Action<IList<byte>, ExtendedRegister> inst2)
        {
            if (op.IsBase)
            {
                inst1(generatedCode, op.BaseRegister);
            }
            else
            {
                inst2(generatedCode, op.ExtendedRegister);
            }
        }

        /// <summary>
        /// Generates code for an one register operand instruction with an int value
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        /// <param name="op">The operand</param>
        /// <param name="value">The value</param>
        /// <param name="inst1">The first instruction</param>
        /// <param name="inst2"> The second instruction</param> 
        /// <typeparam name="T">The type of the value</typeparam>
        private static void GenerateOneRegisterWithValueInstruction<T>(IList<byte> generatedCode, IntRegister op, T value,
            Action<IList<byte>, Register, T> inst1, Action<IList<byte>, ExtendedRegister, T> inst2)
        {
            if (op.IsBase)
            {
                inst1(generatedCode, op.BaseRegister, value);
            }
            else
            {
                inst2(generatedCode, op.ExtendedRegister, value);
            }
        }

        /// <summary>
        /// Generates code for an one memory operand instruction with an int value
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        /// <param name="op">The operand</param>
        /// <param name="value">The value</param>
        /// <param name="inst1">The first instruction</param>
        /// <param name="inst2">The second instruction</param>
        private static void GenerateOneMemoryOperandWithValueInstruction(IList<byte> generatedCode, MemoryOperand op, int value,
            Action<IList<byte>, Register, int, int> inst1, Action<IList<byte>, ExtendedRegister, int, int> inst2)
        {
            if (op.Register.IsBase)
            {
                inst1(generatedCode, op.Register.BaseRegister, op.Offset, value);
            }
            else
            {
                inst2(generatedCode, op.Register.ExtendedRegister, op.Offset, value);
            }
        }

        /// <summary>
        /// Generates code for an instruction with a register destination and memory source
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        /// <param name="op1">The first operand</param>
        /// <param name="op2">The second operand</param>
        /// <param name="inst1">The first instruction</param>
        /// <param name="inst2">The second instruction</param> 
        /// <param name="inst3">The third instruction</param>
        /// <param name="inst4">The fourth instruction</param> 
        private static void GenerateSourceMemoryInstruction(
            IList<byte> generatedCode,
            IntRegister op1,
            MemoryOperand op2,
            Action<IList<byte>, Register, Register, int> inst1,
            Action<IList<byte>, ExtendedRegister, ExtendedRegister, int> inst2,
            Action<IList<byte>, Register, ExtendedRegister, int> inst3,
            Action<IList<byte>, ExtendedRegister, Register, int> inst4)
        {
            if (op1.IsBase && op2.Register.IsBase)
            {
                inst1(generatedCode, op1.BaseRegister, op2.Register.BaseRegister, op2.Offset);
            }
            else if (!op1.IsBase && !op2.Register.IsBase)
            {
                inst2(generatedCode, op1.ExtendedRegister, op2.Register.ExtendedRegister, op2.Offset);
            }
            else if(op1.IsBase && !op2.Register.IsBase)
            {
                inst3(generatedCode, op1.BaseRegister, op2.Register.ExtendedRegister, op2.Offset);
            }
            else
            {
                inst4(generatedCode, op1.ExtendedRegister, op2.Register.BaseRegister, op2.Offset);
            }
        }

        /// <summary>
        /// Generates code for an instruction with a register destination and memory source
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        /// <param name="op1">The first operand</param>
        /// <param name="op2">The second operand</param>
        /// <param name="inst1">The first instruction</param>
        /// <param name = "inst2" > The second instruction</param> 
        /// <typeparam name="T">Te type of the first operand</typeparam>
        private static void GenerateSourceMemoryInstruction<T>(
            IList<byte> generatedCode,
            T op1,
            MemoryOperand op2,
            Action<IList<byte>, T, Register, int> inst1,
            Action<IList<byte>, T, ExtendedRegister, int> inst2)
        {
            if (op2.Register.IsBase)
            {
                inst1(generatedCode, op1, op2.Register.BaseRegister, op2.Offset);
            }
            else
            {
                inst2(generatedCode, op1, op2.Register.ExtendedRegister, op2.Offset);
            }
        }

        /// <summary>
        /// Generates code for an instruction with a memory destination and register source
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        /// <param name="op1">The first operand</param>
        /// <param name="op2">The second operand</param>
        /// <param name="inst1">The first instruction</param>
        /// <param name="inst2">The second instruction</param>
        /// <param name="inst3">The third instruction</param>
        /// <param name="inst4">The fourth instruction</param>
        private static void GenerateDestinationMemoryInstruction(
            IList<byte> generatedCode,
            MemoryOperand op1,
            IntRegister op2,
            Action<IList<byte>, Register, int, Register> inst1,
            Action<IList<byte>, ExtendedRegister, int, ExtendedRegister> inst2,
            Action<IList<byte>, Register, int, ExtendedRegister> inst3,
            Action<IList<byte>, ExtendedRegister, int, Register> inst4)
        {
            if (op1.Register.IsBase && op2.IsBase)
            {
                inst1(generatedCode, op1.Register.BaseRegister, op1.Offset, op2.BaseRegister);
            }
            else if (!op1.Register.IsBase && !op2.IsBase)
            {
                inst2(generatedCode, op1.Register.ExtendedRegister, op1.Offset, op2.ExtendedRegister);
            }
            else if (op1.Register.IsBase && !op2.IsBase)
            {
                inst3(generatedCode, op1.Register.BaseRegister, op1.Offset, op2.ExtendedRegister);
            }
            else
            {
                inst4(generatedCode, op1.Register.ExtendedRegister, op1.Offset, op2.BaseRegister);
            }
        }

        /// <summary>
        /// Generates code for an instruction with a memory destination and register source
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        /// <param name="op1">The first operand</param>
        /// <param name="op2">The second operand</param>
        /// <param name="inst1">The first instruction</param>
        /// <param name="inst2">The second instruction</param>
        /// <typeparam name="T">The type of the second operand</typeparam>
        private static void GenerateDestinationMemoryInstruction<T>(
            IList<byte> generatedCode,
            MemoryOperand op1,
            T op2,
            Action<IList<byte>, Register, int, T> inst1,
            Action<IList<byte>, ExtendedRegister, int, T> inst2)
        {
            if (op1.Register.IsBase)
            {
                inst1(generatedCode, op1.Register.BaseRegister, op1.Offset, op2);
            }
            else
            {
                inst2(generatedCode, op1.Register.ExtendedRegister, op1.Offset, op2);
            }
        }

        /// <summary>
        /// Adds the second register to the first
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Add(IntRegister destination, IntRegister source)
        {
            GenerateTwoRegistersInstruction(
                generatedCode,
                destination,
                source,
                (gen, x, y) => RawAssembler.AddRegisterToRegister(gen, x, y),
                RawAssembler.AddRegisterToRegister,
                RawAssembler.AddRegisterToRegister,
                RawAssembler.AddRegisterToRegister);
        }

        /// <summary>
        /// Adds the given int to the register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="value">The value</param>
        public void Add(IntRegister destination, int value)
        {
            GenerateOneRegisterWithValueInstruction(
                generatedCode,
                destination,
                value,
                (gen, x, y) => RawAssembler.AddIntToRegister(gen, x, y),
                RawAssembler.AddIntToRegister);
        }

        /// <summary>
        /// Adds the memory operand to the register
        /// </summary>
        /// <param name="destination">The destination register</param>
        /// <param name="source">The source memory</param>
        public void Add(IntRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.AddMemoryRegisterWithOffsetToRegister,
                RawAssembler.AddMemoryRegisterWithOffsetToRegister,
                RawAssembler.AddMemoryRegisterWithOffsetToRegister,
                RawAssembler.AddMemoryRegisterWithOffsetToRegister);
        }

        /// <summary>
        /// Adds register to the memory operand
        /// </summary>
        /// <param name="destination">The destination memory</param>
        /// <param name="source">The source register</param>
        public void Add(MemoryOperand destination, IntRegister source)
        {
            GenerateDestinationMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.AddRegisterToMemoryRegisterWithOffset,
                RawAssembler.AddRegisterToMemoryRegisterWithOffset,
                RawAssembler.AddRegisterToMemoryRegisterWithOffset,
                RawAssembler.AddRegisterToMemoryRegisterWithOffset);
        }

        /// <summary>
        /// Adds the second register to the first
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Add(FloatRegister destination, FloatRegister source)
        {
            RawAssembler.AddRegisterToRegister(generatedCode, destination, source);
        }

        /// <summary>
        /// Adds the memory operand to the register
        /// </summary>
        /// <param name="destination">The destination register</param>
        /// <param name="source">The source memory</param>
        public void Add(FloatRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.AddMemoryRegisterWithIntOffsetToRegister,
                RawAssembler.AddMemoryRegisterWithIntOffsetToRegister);
        }

        /// <summary>
        /// Subtracts the second register to the first
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Sub(IntRegister destination, IntRegister source)
        {
            GenerateTwoRegistersInstruction(
                generatedCode,
                destination,
                source,
                (gen, x, y) => RawAssembler.SubRegisterFromRegister(gen, x, y),
                RawAssembler.SubRegisterFromRegister,
                RawAssembler.SubRegisterFromRegister,
                RawAssembler.SubRegisterFromRegister);
        }

        /// <summary>
        /// Subtracts the memory operand from the register
        /// </summary>
        /// <param name="destination">The destination register</param>
        /// <param name="source">The source memory</param>
        public void Sub(IntRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.SubMemoryRegisterWithOffsetFromRegister,
                RawAssembler.SubMemoryRegisterWithOffsetFromRegister,
                RawAssembler.SubMemoryRegisterWithOffsetFromRegister,
                RawAssembler.SubMemoryRegisterWithOffsetFromRegister);
        }

        /// <summary>
        /// Subtrats register from the memory operand
        /// </summary>
        /// <param name="destination">The destination memory</param>
        /// <param name="source">The source register</param>
        public void Sub(MemoryOperand destination, IntRegister source)
        {
            GenerateDestinationMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.SubRegisterFromMemoryRegisterWithOffset,
                RawAssembler.SubRegisterFromMemoryRegisterWithOffset,
                RawAssembler.SubRegisterFromMemoryRegisterWithOffset,
                RawAssembler.SubRegisterFromMemoryRegisterWithOffset);
        }

        /// <summary>
        /// Subtracts the given value from the register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="value">The value to subtract</param>
        public void Sub(IntRegister destination, int value)
        {
            GenerateOneRegisterWithValueInstruction(
                generatedCode,
                destination,
                value,
                (gen, x, y) => RawAssembler.SubIntFromRegister(gen, x, y),
                RawAssembler.SubIntFromRegister);
        }

        /// <summary>
        /// Subtracts the second register to the first
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Sub(FloatRegister destination, FloatRegister source)
        {
            RawAssembler.SubRegisterFromRegister(generatedCode, destination, source);
        }

        /// <summary>
        /// Subtracts the memory operand from the register
        /// </summary>
        /// <param name="destination">The destination register</param>
        /// <param name="source">The source memory</param>
        public void Sub(FloatRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.SubMemoryRegisterWithIntOffsetFromRegister,
                RawAssembler.SubMemoryRegisterWithIntOffsetFromRegister);
        }

        /// <summary>
        /// Multiplies the second register by the first
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Multiply(IntRegister destination, IntRegister source)
        {
            GenerateTwoRegistersInstruction(
                generatedCode,
                destination,
                source,
                (gen, x, y) => RawAssembler.MultiplyRegisterByRegister(gen, x, y),
                RawAssembler.MultiplyRegisterByRegister,
                RawAssembler.MultiplyRegisterByRegister,
                RawAssembler.MultiplyRegisterByRegister);
        }

        /// <summary>
        /// Multiplies the memory operand by the register
        /// </summary>
        /// <param name="destination">The destination register</param>
        /// <param name="source">The source memory</param>
        public void Multiply(IntRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.MultiplyMemoryRegisterWithOffsetByRegister,
                RawAssembler.MultiplyMemoryRegisterWithOffsetByRegister,
                RawAssembler.MultiplyMemoryRegisterWithOffsetByRegister,
                RawAssembler.MultiplyMemoryRegisterWithOffsetByRegister);
        }

        /// <summary>
        /// Multiplies the second register by the first
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Multiply(FloatRegister destination, FloatRegister source)
        {
            RawAssembler.MultiplyRegisterByRegister(generatedCode, destination, source);
        }

        /// <summary>
        /// Multiplies the memory operand by the register
        /// </summary>
        /// <param name="destination">The destination register</param>
        /// <param name="source">The source memory</param>
        public void Multiply(FloatRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.MultiplyMemoryRegisterWithIntOffsetByRegister,
                RawAssembler.MultiplyMemoryRegisterWithIntOffsetByRegister);
        }

        /// <summary>
        /// Multiplies the given register by the given constant
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="value">The constant</param>
        public void Multiply(IntRegister destination, int value)
        {
            GenerateOneRegisterWithValueInstruction<int>(
                this.generatedCode,
                destination,
                value,
                (IList<byte> codeGen, Register x, int y) => RawAssembler.MultiplyConstantToRegister(codeGen, x, y),
                RawAssembler.MultiplyConstantToRegister);
        }

        /// <summary>
        /// Divides the rax register with the given register.
        /// This instruction also modifies the rdx register.
        /// </summary>
        /// <param name="source">The source register</param>
        public void Divide(IntRegister source)
        {
            if (source.IsBase)
            {
                RawAssembler.DivideRegisterFromRegister(generatedCode, Register.AX, source.BaseRegister);
            }
            else
            {
                RawAssembler.DivideRegisterFromRegister(generatedCode, Register.AX, source.ExtendedRegister);
            }
        }

        /// <summary>
        /// Divides the rax register with the given memory operand.
        /// This instruction also modifies the rdx register.
        /// </summary>
        /// <param name="source">The source operand</param>
        public void Divide(MemoryOperand source)
        {
            if (source.Register.IsBase)
            {
                RawAssembler.DivideMemoryRegisterWithOffsetFromRegister(generatedCode, Register.AX, source.Register.BaseRegister, source.Offset);
            }
            else
            {
                RawAssembler.DivideMemoryRegisterWithOffsetFromRegister(generatedCode, Register.AX, source.Register.ExtendedRegister, source.Offset);
            }
        }

        /// <summary>
        /// Divides the second register by the first
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Divide(FloatRegister destination, FloatRegister source)
        {
            RawAssembler.DivideRegisterFromRegister(generatedCode, destination, source);
        }

        /// <summary>
        /// Divides the memory operand by the register
        /// </summary>
        /// <param name="destination">The destination register</param>
        /// <param name="source">The source memory</param>
        public void Divide(FloatRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.DivideMemoryRegisterWithIntOffsetFromRegister,
                RawAssembler.DivideMemoryRegisterWithIntOffsetFromRegister);
        }

        /// <summary>
        /// Moves the second register to the first register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Move(IntRegister destination, IntRegister source)
        {
            GenerateTwoRegistersInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.MoveRegisterToRegister,
                RawAssembler.MoveRegisterToRegister,
                RawAssembler.MoveRegisterToRegister,
                RawAssembler.MoveRegisterToRegister);
        }

        /// <summary>
        /// Moves the memory operand to the register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source memory</param>
        /// <param name="dataSize">The size of the data</param>
        public void Move(IntRegister destination, MemoryOperand source, DataSize dataSize = DataSize.Size64)
        {
            switch (dataSize)
            {
                case DataSize.Size8:
                    throw new ArgumentException("Not supported.");
                case DataSize.Size16:
                    throw new ArgumentException("Not supported.");
                case DataSize.Size32:
                    GenerateSourceMemoryInstruction(
                        generatedCode,
                        destination,
                        source,
                        (gen, dest, src, offset) => RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister(gen, dest, src, offset, true),
                        (gen, dest, src, offset) => throw new ArgumentException("Not supported."),
                        (gen, dest, src, offset) => RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister(gen, dest, src, offset, true),
                        (gen, dest, src, offset) => throw new ArgumentException("Not supported."));
                    break;
                case DataSize.Size64:
                    GenerateSourceMemoryInstruction(
                        generatedCode,
                        destination,
                        source,
                        (gen, dest, src, offset) => RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister(gen, dest, src, offset),
                        RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister,
                        (gen, dest, src, offset) => RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister(gen, dest, src, offset),
                        RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister);
                    break;
            }
        }

        /// <summary>
        /// Moves the memory operand to the register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source memory</param>
        public void Move(Register8Bits destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                this.generatedCode,
                destination,
                source,
                RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister,
                RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister);
        }

        /// <summary>
        /// Moves the register to the memory operand
        /// </summary>
        /// <param name="destination">The destination memory</param>
        /// <param name="source">The source</param>
        /// <param name="dataSize">The size of the data</param>
        public void Move(MemoryOperand destination, IntRegister source, DataSize dataSize = DataSize.Size64)
        {
            switch (dataSize)
            {
                case DataSize.Size8:
                    throw new ArgumentException("Not supported.");
                case DataSize.Size16:
                    throw new ArgumentException("Not supported.");
                case DataSize.Size32:
                    GenerateDestinationMemoryInstruction(
                        generatedCode,
                        destination,
                        source,
                        (gen, dest, offset, src) => RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset(gen, dest, offset, src, true),
                        (gen, dest, offset, src) => throw new ArgumentException("Not supported."),
                        (gen, dest, offset, src) => throw new ArgumentException("Not supported."),
                        (gen, dest, offset, src) => RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset(gen, dest, offset, src, true));
                    break;
                case DataSize.Size64:
                    GenerateDestinationMemoryInstruction(
                        generatedCode,
                        destination,
                        source,
                        (gen, dest, offset, src) => RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset(gen, dest, offset, src),
                        RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset,
                        RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset,
                         (gen, dest, offset, src) => RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset(gen, dest, offset, src));
                    break;
            }
        }

        /// <summary>
        /// Moves the register to the memory operand
        /// </summary>
        /// <param name="destination">The destination memory</param>
        /// <param name="source">The source</param>
        public void Move(MemoryOperand destination, Register8Bits source)
        {
            GenerateDestinationMemoryInstruction(
                this.generatedCode,
                destination,
                source,
                RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset,
                RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset);
        }

        /// <summary>
        /// Moves the given int (32-bit) value to the given register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="value">The value</param>
        public void Move(IntRegister destination, int value)
        {
            GenerateOneRegisterWithValueInstruction(
                generatedCode,
                destination,
                value,
                RawAssembler.MoveIntToRegister,
                RawAssembler.MoveIntToRegister);
        }

        /// <summary>
        /// Moves the given long (64-bit) value to the given register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="value">The value</param>
        public void Move(IntRegister destination, long value)
        {
            GenerateOneRegisterWithValueInstruction(
                generatedCode,
                destination,
                value,
                RawAssembler.MoveLongToRegister,
                RawAssembler.MoveLongToRegister);
        }

        /// <summary>
        /// Moves the given int value to the given memory
        /// </summary>
        /// <param name="destination">The destination memory</param>
        /// <param name="value">The value</param>
        public void Move(MemoryOperand destination, int value)
        {
            GenerateOneMemoryOperandWithValueInstruction(
                generatedCode,
                destination,
                value,
                RawAssembler.MoveIntToMemoryRegWithOffset,
                RawAssembler.MoveIntToMemoryRegWithOffset);
        }

        /// <summary>
        /// Moves the second register to the first register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Move(FloatRegister destination, FloatRegister source)
        {
            RawAssembler.MoveRegisterToRegister(generatedCode, destination, source);
        }

        /// <summary>
        /// Moves the memory operand to the register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source memory</param>
        public void Move(FloatRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister,
                RawAssembler.MoveMemoryRegisterWithIntOffsetToRegister);
        }

        /// <summary>
        /// Moves the register to the memory operand
        /// </summary>
        /// <param name="destination">The destination memory</param>
        /// <param name="source">The source</param>
        public void Move(MemoryOperand destination, FloatRegister source)
        {
            GenerateDestinationMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset,
                RawAssembler.MoveRegisterToMemoryRegisterWithIntOffset);
        }

        /// <summary>
        /// Moves the memory operand to the register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source memory</param>
        public void Move(HardwareRegister destination, MemoryOperand source)
        {
            switch (destination.Type)
            {
                case HardwareRegisterType.Int:
                    Move(destination.IntRegister, source);
                    break;
                case HardwareRegisterType.Float:
                    Move(destination.FloatRegister, source);
                    break;
            }
        }

        /// <summary>
        /// Moves the register to the memory operand
        /// </summary>
        /// <param name="destination">The destination memory</param>
        /// <param name="source">The source</param>
        public void Move(MemoryOperand destination, HardwareRegister source)
        {
            switch (source.Type)
            {
                case HardwareRegisterType.Int:
                    Move(destination, source.IntRegister);
                    break;
                case HardwareRegisterType.Float:
                    Move(destination, source.FloatRegister);
                    break;
            }
        }

        /// <summary>
        /// Compares the second register to the first register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Compare(IntRegister destination, IntRegister source)
        {
            GenerateTwoRegistersInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.CompareRegisterToRegister,
                RawAssembler.CompareRegisterToRegister,
                RawAssembler.CompareRegisterToRegister,
                RawAssembler.CompareRegisterToRegister);
        }

        /// <summary>
        /// Compares the memory operand to the register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source memory</param>
        public void Compare(IntRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.CompareRegisterToMemoryRegisterWithOffset,
                RawAssembler.CompareRegisterToMemoryRegisterWithOffset,
                RawAssembler.CompareRegisterToMemoryRegisterWithOffset,
                RawAssembler.CompareRegisterToMemoryRegisterWithOffset);
        }

        /// <summary>
        /// Compares the register to the memory operand
        /// </summary>
        /// <param name="destination">The destination memory</param>
        /// <param name="source">The source</param>
        public void Compare(MemoryOperand destination, IntRegister source)
        {
            GenerateDestinationMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.CompareMemoryRegisterWithOffsetToRegister,
                RawAssembler.CompareMemoryRegisterWithOffsetToRegister,
                RawAssembler.CompareMemoryRegisterWithOffsetToRegister,
                RawAssembler.CompareMemoryRegisterWithOffsetToRegister);
        }

        /// <summary>
        /// Compares the second register to the first register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Compare(FloatRegister destination, FloatRegister source)
        {
            RawAssembler.CompareRegisterToRegister(generatedCode, destination, source);
        }

        /// <summary>
        /// Compares the memory operand to the register
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source memory</param>
        public void Compare(FloatRegister destination, MemoryOperand source)
        {
            GenerateSourceMemoryInstruction(
                generatedCode,
                destination,
                source,
                RawAssembler.CompareRegisterToMemoryRegisterWithOffset,
                RawAssembler.CompareRegisterToMemoryRegisterWithOffset);
        }

        /// <summary>
        /// Applies bitwise AND between the given registers
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        /// <param name="is32Bits">Indicates if the operation is 32-bits</param>
        public void And(IntRegister destination, IntRegister source, bool is32Bits = false)
        {
            GenerateTwoRegistersInstruction(
                generatedCode,
                destination,
                source,
                (gen, x, y) => RawAssembler.AndRegisterToRegister(gen, x, y, is32Bits),
                RawAssembler.AndRegisterToRegister,
                RawAssembler.AndRegisterToRegister,
                RawAssembler.AndRegisterToRegister);
        }

        /// <summary>
        /// Applies bitwise AND between the given register and constant
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="value">The value</param>
        /// <param name="is32Bits">Indicates if the operation is 32-bits</param>
        public void And(IntRegister destination, int value, bool is32Bits = false)
        {
            GenerateOneRegisterWithValueInstruction<int>(
                generatedCode,
                destination,
                value,
                (gen, x, y) => RawAssembler.AndIntToRegister(gen, x, y, is32Bits),
                (gen, x, y) => RawAssembler.AndIntToRegister(gen, x, y));
        }

        /// <summary>
        /// Applies bitwise OR between the given registers
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        /// <param name="is32Bits">Indicates if the operation is 32-bits</param>
        public void Or(IntRegister destination, IntRegister source, bool is32Bits = false)
        {
            GenerateTwoRegistersInstruction(
                generatedCode,
                destination,
                source,
                (gen, x, y) => RawAssembler.OrRegisterToRegister(gen, x, y, is32Bits),
                RawAssembler.OrRegisterToRegister,
                RawAssembler.OrRegisterToRegister,
                RawAssembler.OrRegisterToRegister);
        }

        /// <summary>
        /// Applies bitwise NOT to the given register
        /// </summary>
        /// <param name="register">The register</param>
        /// <param name="is32Bits">Indicates if the operation is 32-bits</param>
        public void Not(IntRegister register, bool is32Bits = false)
        {
            GenerateOneRegisterInstruction(
                generatedCode,
                register,
                (gen, x) => RawAssembler.NotRegister(gen, x),
                (gen, x) => RawAssembler.NotRegister(gen, x));
        }

        /// <summary>
        /// Applies bitwise XOR between the given registers
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="source">The source</param>
        public void Xor(IntRegister destination, IntRegister source)
        {
            GenerateTwoRegistersInstruction(
                generatedCode,
                destination,
                source,
                (gen, x, y) => RawAssembler.XorRegisterToRegister(gen, x, y),
                RawAssembler.XorRegisterToRegister,
                RawAssembler.XorRegisterToRegister,
                RawAssembler.XorRegisterToRegister);
        }

        /// <summary>
        /// Pushes the given register
        /// </summary>
        /// <param name="register">The register</param>
        public void Push(HardwareRegister register)
        {
            switch (register.Type)
            {
                case HardwareRegisterType.Int:
                    Push(register.IntRegister);
                    break;
                case HardwareRegisterType.Float:
                    Push(register.FloatRegister);
                    break;
            }
        }

        /// <summary>
        /// Pushes the given register
        /// </summary>
        /// <param name="register">The register</param>
        public void Push(IntRegister register)
        {
            GenerateOneRegisterInstruction(
                generatedCode,
                register,
                RawAssembler.PushRegister,
                RawAssembler.PushRegister);
        }

        /// <summary>
        /// Pushes the given register
        /// </summary>
        /// <param name="register">The register</param>
        public void Push(FloatRegister register)
        {
            RawAssembler.PushRegister(generatedCode, register);
        }

        /// <summary>
        /// Pushes the given integer
        /// </summary>
        /// <param name="value">The value to push</param>
        public void Push(int value)
        {
            RawAssembler.PushInt(generatedCode, value);
        }

        /// <summary>
        /// Pops the given register
        /// </summary>
        /// <param name="register">The register</param>
        public void Pop(HardwareRegister register)
        {
            switch (register.Type)
            {
                case HardwareRegisterType.Int:
                    Pop(register.IntRegister);
                    break;
                case HardwareRegisterType.Float:
                    Pop(register.FloatRegister);
                    break;
            }
        }

        /// <summary>
        /// Pops the given register
        /// </summary>
        /// <param name="register">The register</param>
        public void Pop(IntRegister register)
        {
            GenerateOneRegisterInstruction(
                generatedCode,
                register,
                RawAssembler.PopRegister,
                RawAssembler.PopRegister);
        }

        /// <summary>
        /// Pops the given register
        /// </summary>
        /// <param name="register">The register</param>
        public void Pop(FloatRegister register)
        {
            RawAssembler.PopRegister(generatedCode, register);
        }

        /// <summary>
        /// Pops the top operand
        /// </summary>
        public void Pop()
        {
            RawAssembler.AddByteToReg(generatedCode, Register.SP, RawAssembler.RegisterSize);
        }

        /// <summary>
        /// Jumps to the given target
        /// </summary>
        /// <param name="condition">The jump condition</param>
        /// <param name="target">The target relative to the end of the generated instruction.</param>
        /// <param name="unsignedComparison">Indicates if to use an unsigned comparison</param>
        public void Jump(JumpCondition condition, int target, bool unsignedComparison = false)
        {
            switch (condition)
            {
                case JumpCondition.Always:
                    RawAssembler.Jump(generatedCode, target);
                    break;
                case JumpCondition.Equal:
                    RawAssembler.JumpEqual(generatedCode, target);
                    break;
                case JumpCondition.NotEqual:
                    RawAssembler.JumpNotEqual(generatedCode, target);
                    break;
                case JumpCondition.LessThan:
                    if (unsignedComparison)
                    {
                        RawAssembler.JumpLessThanUnsigned(generatedCode, target);
                    }
                    else
                    {
                        RawAssembler.JumpLessThan(generatedCode, target);
                    }
                    break;
                case JumpCondition.LessThanOrEqual:
                    if (unsignedComparison)
                    {
                        RawAssembler.JumpLessThanOrEqualUnsigned(generatedCode, target);
                    }
                    else
                    {
                        RawAssembler.JumpLessThanOrEqual(generatedCode, target);
                    }
                    break;
                case JumpCondition.GreaterThan:
                    if (unsignedComparison)
                    {
                        RawAssembler.JumpGreaterThanUnsigned(generatedCode, target);
                    }
                    else
                    {
                        RawAssembler.JumpGreaterThan(generatedCode, target);
                    }
                    break;
                case JumpCondition.GreaterThanOrEqual:
                    if (unsignedComparison)
                    {
                        RawAssembler.JumpGreaterThanOrEqualUnsigned(generatedCode, target);
                    }
                    else
                    {
                        RawAssembler.JumpGreaterThanOrEqual(generatedCode, target);
                    }
                    break;
            }
        }

        /// <summary>
        /// Calls the given function where the entry points is in a register
        /// </summary>
        /// <param name="addressRegister">The register where the address is</param>
        public void CallInRegister(IntRegister addressRegister)
        {
            if (addressRegister.IsBase)
            {
                RawAssembler.CallInRegister(generatedCode, addressRegister.BaseRegister);
            }
            else
            {
                RawAssembler.CallInRegister(generatedCode, addressRegister.ExtendedRegister);
            }
        }

        /// <summary>
        /// Calls the given function
        /// </summary>
        /// <param name="relativeAddress">The relative address</param>
        public void Call(int relativeAddress)
        {
            RawAssembler.Call(generatedCode, relativeAddress);
        }

        /// <summary>
        /// Makes a return from the current function
        /// </summary>
        public void Return()
        {
            RawAssembler.Return(generatedCode);
        }
    }
}
