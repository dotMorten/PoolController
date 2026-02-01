using System.Buffers;
using System.Text;

namespace Pentair;


public class Message
{
    public byte Destination { get; set; }
    public byte Source { get; set; }
    public byte Command { get; set; }
    public byte[] Data { get; set; }
    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Dst={0:X2} Src={1:X2} Cmd={2:X2} DataLen={3} Data: ", Destination, Source, Command, Data.Length);
        foreach (var b in Data)
        {
            sb.AppendFormat("{0:X2} ", b);
        }
        return sb.ToString();
    }
    protected ushort GetUInt16(byte offset)
    {
        var slice = Data.AsSpan(offset, 2);
        return (ushort)(slice[0] * 256 + slice[1]);
    }
    protected byte GetByte(byte offset)
    {
        return Data[offset];
    }
}


public class StatusMessage : Message
{
    public PumpRunning Run => (PumpRunning)GetByte(0); //  04 if the pump has been deactivated, 0A if it is ready to pump
    public PumpMode Mode => (PumpMode)GetByte(1); // the index of the program currently running
    public PumpState State => (PumpState)GetByte(2); //  pumping state, normally 02. It would be 00 in the event of an error, and 01 or 04 during priming, but we haven't had a chance to check this.
    public ushort Power => GetUInt16(3);
    public ushort Rpm => GetUInt16(5);
    public byte Gpm => GetByte(7);
    public byte Ppc => GetByte(8);
    public byte Error => GetByte(10);
    public TimeSpan Timer => new TimeSpan(GetByte(11), GetByte(12), 0);
    public TimeOnly Clock => new TimeOnly(GetByte(13), GetByte(14), 0);


    public override string ToString()
    {
        return $"{base.ToString()}\n\tRun: {Run}\n\tMode: {Mode}\n\tPmp: {State}\n\tPower: {Power}\n\tRPM: {Rpm}\n\tGPM: {Gpm}\n\tPPC: {Ppc}\n\tError: {Error}\n\tTimer: {Timer}\n\tTime: {Clock}";
    }
}
public enum PumpRunning : byte
{
    Started = 0x0a,
    Stopped = 0x04
}
public enum PumpMode : byte
{
    Filter = 0x00,
    Local1 = 0x01,
    Local2 = 0x02,
    Local3 = 0x03,
    Local4 = 0x04,
    External1 = 0x09,
    External2 = 0x0A,
    External3 = 0x0B,
    External4 = 0x0C,
    Timeout = 0x0E,
    Priming = 0x11,
    QuickClean = 0x0D,
    Unknown = 0xFF,
}
public enum PumpState : byte
{
    FaultMode = 0x00,
    Priming = 0x01,
    Normal = 0x02,
    SystemPriming = 0x04,
}
