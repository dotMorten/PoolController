using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;

namespace Pentair;

public class Client : IDisposable
{
    public const byte Pump1 = 0x60;
    private const byte SourceAddress = 0x20;
    public readonly static byte[] PanelControlOff = new byte[] { SourceAddress, 0x04, 0x01, 0xFF };
    public readonly static byte[] PanelControlOn = new byte[] { SourceAddress, 0x04, 0x01, 0x00 };
    public readonly static byte[] RequestStatus = new byte[] { SourceAddress, 0x07, 0x00 };
    public readonly static byte[] StartProgram2 = new byte[] { SourceAddress, 0x01, 0x04, 0x03, 0x21, 0x00, 0x20 };
    public readonly static byte[] StopCommand = new byte[] { SourceAddress, 0x01, 0x06, 0x01, 0x0A };
    public readonly static byte[] StartCommand = new byte[] { SourceAddress, 0x01, 0x06, 0x01, 0x04 };


    static byte[] preamble = new byte[] { 0xff, 0x00, 0xff, 0xA5, 0x00 };
    private readonly System.IO.Ports.SerialPort port;
    private readonly Pipe pipe;
    System.Device.Gpio.GpioController? gpio;

    bool isDisposed;
    public Client(string serialPort)
    {
        Log("Opening RS485 Port on  + serialPort");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            gpio = new System.Device.Gpio.GpioController();
            gpio.OpenPin(18, System.Device.Gpio.PinMode.Output);
            gpio.Write(18, System.Device.Gpio.PinValue.Low);
        }
        port = new System.IO.Ports.SerialPort(serialPort);
        pipe = new Pipe();
        port.BaudRate = 9600;
        port.DataReceived += OnSerialPortDataReceived;
        port.Open();
        Log("Opened RS485 Port");
        _ = ReadPipeAsync(pipe.Reader);
    }

    public void Dispose()
    {
        isDisposed = true;
        port.Dispose();
    }

    private async void OnSerialPortDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
    {
        int i = 0;
        while (port.BytesToRead > 0)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(port.BytesToRead);
            var memory = pipe.Writer.GetMemory(buffer.Length);
            int count = port.Read(buffer, i, buffer.Length);
            buffer.CopyTo(memory);
            pipe.Writer.Advance(count);
            ArrayPool<byte>.Shared.Return(buffer);
        }
        FlushResult result = await pipe.Writer.FlushAsync();
    }

    private async Task ReadPipeAsync(PipeReader reader)
    {
        while (!isDisposed)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryReadMessage(ref buffer, out ReadOnlySequence<byte> msg))
            {
                ProcessMessage(msg);
            }
            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await reader.CompleteAsync();
    }

    private void ProcessMessage(ReadOnlySequence<byte> msg)
    {
        var reader = new SequenceReader<byte>(msg);
        if (reader.TryRead(out byte lpb) && lpb == 0xA5 &&
            reader.TryRead(out byte sub) &&
            reader.TryRead(out byte dst) &&
            reader.TryRead(out byte src) &&
            reader.TryRead(out byte cfi) &&
            reader.TryRead(out byte len))
        {
            var data = msg.Slice(6, len);
            var checksum = msg.Slice(6 + len, 2);
            ushort expectedChecksum = 0xA5;
            expectedChecksum += sub;
            expectedChecksum += dst;
            expectedChecksum += src;
            expectedChecksum += cfi;
            expectedChecksum += len;
            for (int i = 0; i < len; i++)
            {
                if (!reader.TryRead(out byte b))
                {
                    break;
                }
                expectedChecksum += b;
            }
            var checksumBytes = new byte[2];
            BitConverter.GetBytes(expectedChecksum).CopyTo(checksumBytes, 0);
            if (checksum.First.Span[0] != checksumBytes[1] ||
               checksum.First.Span[1] != checksumBytes[0])
            {
                return; // Checksum mismatch
            }
            Message message;
            if (cfi == 0x07)
            {
                message = new StatusMessage
                {
                    Destination = dst,
                    Source = src,
                    Command = cfi,
                    Data = data.ToArray()
                };
            }
            else
            {
                message = new Message()
                {
                    Destination = dst,
                    Source = src,
                    Command = cfi,
                    Data = data.ToArray()
                };
            }
            MessageReceived?.Invoke(null, message);
            Log(message);
        }
    }

    private static void Log(object message)
    {
#if DEBUG
        if (Debugger.IsAttached)
            Debug.WriteLine(message.ToString());
        else if (!Console.IsInputRedirected)
            Console.WriteLine(message.ToString());
#endif
    }

    public Task Stop(byte pumpId)
    {
        return SendCommandAsync(pumpId, StopCommand);
    }
    public Task Start(byte pumpId)
    {
        return SendCommandAsync(pumpId, StartCommand);
    }

    public event EventHandler<Message>? MessageReceived;


    private bool TryReadMessage(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> msg)
    {
        // Look for a starting point in the buffer.
        // A Pentair message starts with 0xff 0x00 0xff followed by the actual message.
        // The first 5 bytes are the preamble and destination. 6th byte is the length of the message.
        // followed by the data and 2 byte checksum.
        // This method will find an entire message and add it to 'msg' and advance the buffer
        var reader = new SequenceReader<byte>(buffer);
        // Look for the exact sequence 0xff, 0x00, 0xff
        while(reader.Remaining > 3) {
            if (reader.TryAdvanceTo(0xff, advancePastDelimiter: true))
            {
                var tempReader = reader;
                if (tempReader.TryRead(out byte b1) && b1 == 0x00 &&
                    tempReader.TryRead(out byte b2) && b2 == 0xff)
                {
                // Advance the main reader past the sequence
                reader.Advance(2);
                var start = reader.Position;
                if (reader.TryReadExact(5, out _) &&
                    reader.TryRead(out byte len) &&
                    reader.TryReadExact(len + 2, out _))
                {
                    msg = buffer.Slice(start, reader.Position);
                    buffer = buffer.Slice(buffer.GetPosition(0, reader.Position));

                    return true;
                }
                }
            }
            else {
                break;
            }
        }
        msg = default;
        return false;
    }
    object sendLock = new object();
    public async Task SendCommandAsync(byte destination, byte[] msg)
    {
        //lock (sendLock)
        {
            gpio?.Write(18, System.Device.Gpio.PinValue.High);
            await Task.Delay(10).ConfigureAwait(false);
            port.Write(preamble, 0, preamble.Length);
            port.Write(new byte[] { destination }, 0, 1);
            port.Write(msg, 0, msg.Length);
            ushort checksum = 0;
            checksum += 0xa5;
            checksum += destination;
            for (int i = 0; i < msg.Length; i++)
            {
                checksum += msg[i];
            }
            byte[] checksumbuf = new byte[2];
            BitConverter.GetBytes(checksum).CopyTo(checksumbuf, 0);
            port.Write(checksumbuf, 1, 1);
            port.Write(checksumbuf, 0, 1);
            await Task.Delay(10).ConfigureAwait(false);
            gpio?.Write(18, System.Device.Gpio.PinValue.Low);
        }
    }

    public async Task<StatusMessage> GetStatusAsync(byte pump, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<StatusMessage>();
        EventHandler<Message> handler;
        handler = (s, e) =>
        {
            if (e is StatusMessage statusMessage && statusMessage.Source == pump && statusMessage.Destination == SourceAddress)
            {
                tcs.SetResult(statusMessage);
            }
        };
        MessageReceived += handler;
        using (cancellationToken.Register(() =>
        {
            tcs.TrySetCanceled();
        }))
        
        await SendCommandAsync(pump, RequestStatus);
        var timeoutTask = Task.Delay(2000, cancellationToken).ContinueWith(t => tcs.TrySetException(new TimeoutException("Timeout waiting for status message")));
        try
        {
            return await tcs.Task;
        }
        finally
        {
            MessageReceived -= handler;
        }
    }
}
