using System;

namespace Quartz.Runtime.Types
{
    internal struct QPointer
    {
        public long Address { get; }

        public QPointer(long address)
        {
            Address = address;
        }

        public override string ToString()
        {
            return $"0x{Address:X}";
        }

        public override bool Equals(object obj)
        {
            if (obj is QPointer other)
            {
                return Address == other.Address;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }

        
        public static QPointer operator +(QPointer ptr, int offset) => new QPointer(ptr.Address + offset);
        public static QPointer operator -(QPointer ptr, int offset) => new QPointer(ptr.Address - offset);
    }
}


