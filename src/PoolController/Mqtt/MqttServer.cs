using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using NoeticTools.Net2HassMqtt;
using NoeticTools.Net2HassMqtt.Configuration;
using NoeticTools.Net2HassMqtt.Configuration.Building;

namespace PoolController.Mqtt;

public class MqttServer
{
    INet2HassMqttBridge bridge;

    private MqttServer(INet2HassMqttBridge bridge)
    {
        this.bridge = bridge;
    }

    internal static async Task<MqttServer> StartServer(string address, string username, string password)
    {
        var appConfig = new ConfigurationBuilder().AddInMemoryCollection().Build();
        appConfig.Providers.First();
        var mqttBrokerSection = appConfig.GetSection("MqttBroker");
        mqttBrokerSection["Address"] = address;
        mqttBrokerSection["Username"] = username;
        mqttBrokerSection["Password"] = password;
        var poolModel = PoolService.Instance.PumpStatus;
        var chlorinatorModel = PoolService.Instance.ChlorinatorStatus;

        var device1 = Devices.BuildPump(poolModel, "Pump1");
        var device2 = Devices.BuildChlorinator(chlorinatorModel);

        var mqttClientOptions = HassMqttClientFactory.CreateQuickStartOptions("pool_controller", appConfig);
        var bridge = new BridgeConfiguration()
                     .WithMqttOptions(mqttClientOptions)
                     .HasDevice(device1)
                     //.HasDevice(device2)
                     .Build();

            // Start the bridge
            await bridge.StartAsync();
        return new MqttServer(bridge);
    }

    internal async Task StopAsync()
    {
        await bridge.StopAsync();
    }
}
