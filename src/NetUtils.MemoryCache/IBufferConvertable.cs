using System;

namespace NetUtils.MemoryCache
{
    public interface IBufferConvertable
    {
        Memory<byte> ToBuffer();

        void LoadFromBuffer(Memory<byte> buffer);
    }
}
