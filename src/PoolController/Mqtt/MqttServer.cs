using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using NoeticTools.Net2HassMqtt;
using NoeticTools.Net2HassMqtt.Configuration;
using NoeticTools.Net2HassMqtt.Configuration.Building;

namespace PoolController.Mqtt;

internal static class MqttServer
{
    static INet2HassMqttBridge? bridge;

    internal static async Task StartServer(string address, string username, string password)
    {
        if (bridge != null)
        {
            throw new InvalidOperationException("Bridge is already configured.");
        }
        var appConfig = new ConfigurationBuilder().AddInMemoryCollection().Build();
        appConfig.Providers.First();
        var mqttBrokerSection = appConfig.GetSection("MqttBroker");
        mqttBrokerSection["Address"] = address;
        mqttBrokerSection["Username"] = username;
        mqttBrokerSection["Password"] = password;
        var poolModel = new PoolPumpModel();
        var chlorinatorModel = new ChlorinatorModel();

        var device1 = Devices.BuildPump(poolModel, "pool_pump_a5de12fa");
        var device2 = Devices.BuildChlorinator(chlorinatorModel);

        var mqttClientOptions = HassMqttClientFactory.CreateQuickStartOptions("pool_controller", appConfig);
        bridge = new BridgeConfiguration()
                     .WithMqttOptions(mqttClientOptions)
                     .HasDevice(device1)
                     .HasDevice(device2)
                     .Build();

        try
        {
            // Start the bridge
            await bridge.StartAsync();
        }
        finally
        {
            await bridge.StopAsync();
            await Task.Delay(100);
        }
    }
    internal static async Task StopAsync()
    {
        if(bridge is null) return;
        await bridge.StopAsync();
        bridge = null;
    }
}
