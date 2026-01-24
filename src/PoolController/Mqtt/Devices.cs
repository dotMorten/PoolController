using System;
using System.Collections.Generic;
using System.Text;
using NoeticTools.Net2HassMqtt.Configuration.Building;
using NoeticTools.Net2HassMqtt.Configuration.UnitsOfMeasurement;

namespace PoolController.Mqtt;

internal static class Devices
{

    public static DeviceBuilder BuildPump(PoolPumpModel model, string id)
    {
        var device = new DeviceBuilder()
            .WithFriendlyName("Pool Pump")
            .WithId(id)
            .WithManufacturer("Pentair")
            .WithModel("IntelliFlo VSF");

        device.HasSwitch(config => config.OnModel(model)
                  .WithFriendlyName("Switch")
                  .WithStatusProperty(nameof(PoolPumpModel.IsOn))
                  .WithNodeId("enabled")
                  .WithCommandMethod(nameof(PoolPumpModel.ToggleOn)))
              .HasFrequencySensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolPumpModel.PumpSpeed))
                  .WithFriendlyName("Pump Speed")
                  .WithIcon("mdi:fan")
                  .WithNodeId("pump_speed")
                  .WithUnitOfMeasurement(FrequencySensorUoM.Hz))
              .HasPowerSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolPumpModel.Power))
                  .WithFriendlyName("Power")
                  .WithNodeId("power")
                  .WithUnitOfMeasurement(PowerSensorUoM.Watts))
              .HasVolumeFlowRateSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolPumpModel.EstimatedFlow))
                  .WithFriendlyName("Estimated Flow")
                  .WithNodeId("estimated_flow")
                  .WithUnitOfMeasurement(VolumeFlowRateSensorUoM.Galpermin))
              .HasTimestampSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolPumpModel.CurrentTime))
                  .WithUnitOfMeasurement(TimestampSensorUoM.None)
                  .WithFriendlyName("Current Time")
                  .WithNodeId("current_time"))
              .HasEnumSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolPumpModel.State))
                  .WithFriendlyName("State")
                  .WithNodeId("state"))
              //.HasEnumSensor(config => config.OnModel(model)
              //  .WithFriendlyName("State")
              //  .WithStatusProperty(nameof(PoolPumpModel.State))
              //  .WithNodeId("state"));


              ;
        return device;
    }

    internal static DeviceBuilder BuildChlorinator(ChlorinatorModel model)
    {
        var device = new DeviceBuilder().WithFriendlyName("Chlorinator")
                                        .WithId("chlorinator_a5de12fa");

        device.HasSensor(config => config.OnModel(model)
                                                   .WithStatusProperty(nameof(ChlorinatorModel.Percentage))
                                                   .WithFriendlyName("Percentage")
                                                   .WithNodeId("percentage")
                                                   .WithUnitOfMeasurement(DefaultSensorUoM.None));
        return device;
    }
}
