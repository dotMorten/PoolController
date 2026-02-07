using System;
using System.Collections.Generic;
using System.Text;
using System.Device.Gpio;

namespace PoolController.Devices;

internal class Actuators
{
    const int actuator1Pin = 1;
    const int actuator2Pin = 7;
    const int actuator3Pin = 8;
    const int actuator4Pin = 25;

    private readonly GpioController controller;

    private Actuators()
    {
        controller = new GpioController();
        controller.SetPinMode(actuator1Pin, PinMode.Output);
        controller.SetPinMode(actuator2Pin, PinMode.Output);
        controller.SetPinMode(actuator3Pin, PinMode.Output);
        controller.SetPinMode(actuator4Pin, PinMode.Output);
    }

    public static Actuators Instance { get; } = new Actuators();

    public void SetActuator(int id, bool on)
    {
        switch (id)
        {
            case 1: Actuator1 = on; break;
            case 2: Actuator2 = on; break;
            case 3: Actuator3 = on; break;
            case 4: Actuator4 = on; break;
        }
    }
    public bool GetActuator(int id)
    {
        switch (id)
        {
            case 1: return Actuator1;
            case 2: return Actuator2;
            case 3: return Actuator3;
            case 4: return Actuator4;
        }
        return false;
    }

    public bool Actuator1
    {
        get => controller.Read(actuator1Pin) == PinValue.High;
        set => controller.Write(actuator1Pin, PinValue.High);
    }
    
    public bool Actuator2
    {
        get => controller.Read(actuator2Pin) == PinValue.High;
        set => controller.Write(actuator2Pin, PinValue.High);
    }
    
    public bool Actuator3
    {
        get => controller.Read(actuator3Pin) == PinValue.High;
        set => controller.Write(actuator3Pin, PinValue.High);
    }

    public bool Actuator4
    {
        get => controller.Read(actuator4Pin) == PinValue.High;
        set => controller.Write(actuator4Pin, PinValue.High);
    }
}
