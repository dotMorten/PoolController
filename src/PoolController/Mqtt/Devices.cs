using System;
using System.Collections.Generic;
using System.Text;
using NoeticTools.Net2HassMqtt.Configuration.Building;
using NoeticTools.Net2HassMqtt.Configuration.UnitsOfMeasurement;
using PoolController.Models;

namespace PoolController.Mqtt;

internal static class Devices
{

    public static DeviceBuilder BuildPump(PoolControllerModel model, string id)
    {
        var device = new DeviceBuilder()
            .WithFriendlyName("Pool Controller")
            .WithId(id)
            .WithManufacturer("Morten Nielsen")
            .WithModel("Pool Controller");

        device.HasSwitch(config => config.OnModel(model)
                  .WithFriendlyName("Pump Switch")
                  .WithStatusProperty(nameof(PoolControllerModel.IsOn))
                  .WithNodeId("pump_enabled")
                  .WithCommandMethod(nameof(PoolControllerModel.ToggleOn)))
              .HasFrequencySensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.PumpSpeed))
                  .WithFriendlyName("Pump Speed")
                  .WithIcon("mdi:fan")
                  .WithNodeId("pump_speed")
                  .WithUnitOfMeasurement(FrequencySensorUoM.Hz))
              .HasPowerSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.Power))
                  .WithFriendlyName("Pump Power")
                  .WithNodeId("pump_power")
                  .WithUnitOfMeasurement(PowerSensorUoM.Watts))
              .HasVolumeFlowRateSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.EstimatedFlow))
                  .WithFriendlyName("Pump Estimated Flow")
                  .WithNodeId("pump_estimated_flow")
                  .WithUnitOfMeasurement(VolumeFlowRateSensorUoM.Galpermin))
              // .HasTimestampSensor(config => config.OnModel(model)
              //     .WithStatusProperty(nameof(PoolPumpModel.Clock))
              //     .WithUnitOfMeasurement(TimestampSensorUoM.None)
              //     .WithFriendlyName("Current Time")
              //     .WithNodeId("current_time"))
              // .HasEnumSensor(config => config.OnModel(model)
              //     .WithStatusProperty(nameof(PoolControllerModel.State))
              //     .WithFriendlyName("Pump State")
              //     .WithNodeId("pump_state"))
              .HasEnumSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.Running))
                  .WithFriendlyName("Pump Running")
                  .WithNodeId("pump_running"))
              .HasSwitch(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.PumpServiceMode))
                  .WithFriendlyName("Pump Service Mode")
                  .WithCommandMethod(nameof(PoolControllerModel.SetPumpServiceMode))
                  .WithNodeId("pump_service_mode"))

              //.HasEnumSensor(config => config.OnModel(model)
              //  .WithFriendlyName("State")
              //  .WithStatusProperty(nameof(PoolPumpModel.State))
              //  .WithNodeId("state"));

              .HasEnumSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.SolarHeatingMode))
                  .WithFriendlyName("Solar Heating Mode")
                  .WithNodeId("solar_heating_mode"))
              .HasTemperatureSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.AirTemperature))
                  .WithFriendlyName("Air Temperature")
                  .WithNodeId("air_temperature")
                  .WithUnitOfMeasurement(TemperatureSensorUoM.DegreesFahrenheit))
              .HasTemperatureSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.WaterTemperature))
                  .WithFriendlyName("Water Temperature")
                  .WithNodeId("water_temperature")
                  .WithUnitOfMeasurement(TemperatureSensorUoM.DegreesFahrenheit))
              .HasTemperatureSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.ReturnWaterTemperature))
                  .WithFriendlyName("Water Return Temperature")
                  .WithNodeId("water_return_temperature")
                  .WithUnitOfMeasurement(TemperatureSensorUoM.DegreesFahrenheit))
              .HasTemperatureSensor(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.SolarAirTemperature))
                  .WithFriendlyName("Solar Air Temperature")
                  .WithNodeId("solar_air_temperature")
                  .WithUnitOfMeasurement(TemperatureSensorUoM.DegreesFahrenheit))
              .HasTemperatureNumber(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.SolarTargetTemperature))
                  .WithFriendlyName("Solar Target Temperature")
                  .WithNodeId("solar_target_temperature")
                  .WithCommandMethod(nameof(PoolControllerModel.SetSolarTargetTemperature))
                  .WithMinimum(50).WithMaximum(120)
                  .WithUnitOfMeasurement(TemperatureNumberUoM.DegreesFahrenheit))
              .HasSwitch(config => config.OnModel(model)
                  .WithStatusProperty(nameof(PoolControllerModel.VacuumEnabled))
                  .WithCommandMethod(nameof(PoolControllerModel.SetVacuumEnabled))
                  .WithFriendlyName("Vacuum")
                  .WithNodeId("vacuum"))

              .HasSensor(config => config.OnModel(model)
                   .WithStatusProperty(nameof(PoolControllerModel.ChlorinatorPercentage))
                   .WithFriendlyName("Chlorinator Percentage")
                   .WithNodeId("chlorinator_percentage")
                   .WithUnitOfMeasurement(DefaultSensorUoM.None))
              .HasSensor(config => config.OnModel(model)
                   .WithStatusProperty(nameof(PoolControllerModel.ChlorinatorSaltLevel))
                   .WithFriendlyName("Chlorinator Salt Level")
                   .WithNodeId("chlorinator_salt_level")
                   .WithUnitOfMeasurement(DefaultSensorUoM.None));

        ;
        return device;
    }
}
