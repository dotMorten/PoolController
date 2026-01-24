using System.Buffers;
using System.Text;

namespace Pentair;


public class Message
{
    public byte Destination { get; set; }
    public byte Source { get; set; }
    public byte Command { get; set; }
    public ReadOnlySequence<byte> Data { get; set; }
    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Dst={0:X2} Src={1:X2} Cmd={2:X2} DataLen={3} Data: ", Destination, Source, Command, Data.Length);
        foreach (var segment in Data)
        {
            foreach (var b in segment.Span)
            {
                sb.AppendFormat("{0:X2} ", b);
            }
        }
        return sb.ToString();
    }
    protected ushort GetUInt16(byte offset)
    {
        var slice =
        Data.Slice(offset, 2);
        return (ushort)(slice.First.Span[0] * 256 + Data.First.Span[1]);
    }
    protected byte GetByte(byte offset)
    {
        return Data.First.Span[offset];
    }
}


public class StatusMessage : Message
{
    public byte Run => GetByte(0); //  04 if the pump has been deactivated, 0A if it is ready to pump
    public byte Mode => GetByte(1); // the index of the program currently running
    public byte Pmp => GetByte(2); //  pumping state, normally 02. It would be 00 in the event of an error, and 01 or 04 during priming, but we haven't had a chance to check this.
    public ushort Power => GetUInt16(3);
    public ushort Rpm => GetUInt16(5);
    public byte Gpm => GetByte(7);
    public byte Ppc => GetByte(8);
    public byte Error => GetByte(10);
    public TimeSpan Timer => new TimeSpan(GetByte(11), GetByte(12), 0);
    public TimeOnly Clock => new TimeOnly(GetByte(13), GetByte(14), 0);


    public override string ToString()
    {
        return $"{base.ToString()}\n\tRun: {Run}\n\tMode: {Mode}\n\tPmp: {Pmp}\n\tPower: {Power}\n\tRPM: {Rpm}\n\tGPM: {Gpm}\n\tPPC: {Ppc}\n\tError: {Error}\n\tTimer: {Timer}\n\tTime: {Clock}";
    }
}