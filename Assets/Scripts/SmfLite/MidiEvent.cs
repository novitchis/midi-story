using System;
using System.Linq;

namespace SmfLite
{
    // MIDI event struct.
    public struct MidiEvent : IMidiEvent
    {
        public byte status;
        public byte data1;
        public byte data2;

        public MidiEvent (byte status, byte data1, byte data2)
        {
            this.status = status;
            this.data1 = data1;
            this.data2 = data2;
        }

        public override string ToString ()
        {
            return "[" + status.ToString ("X") + "," + data1.ToString ("X") + "," + data2.ToString ("X") + "]";
        }
    }

    public struct MidiMetaEvent : IMidiEvent
    {
        public byte type;
        public byte[] bytes;

        public MidiMetaEvent(byte type, byte[] bytes)
        {
            this.type = type;
            this.bytes = bytes;
        }

        public override string ToString()
        {
            return "[FF " + type.ToString("X") + "," + String.Join(" ", bytes.Select(b => b.ToString("X"))) + "]";
        }
    }

    public interface IMidiEvent
    {
    }
}