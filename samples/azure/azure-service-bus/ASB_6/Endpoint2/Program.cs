﻿using System;
using NServiceBus;

class Program
{

    static void Main()
    {
        Console.Title = "Samples.Azure.ServiceBus.Endpoint2";
        var busConfiguration = new BusConfiguration();
        busConfiguration.EndpointName("Samples.Azure.ServiceBus.Endpoint2");
        busConfiguration.UseSerialization<JsonSerializer>();
        busConfiguration.EnableInstallers();
        busConfiguration.ScaleOut().UseSingleBrokerQueue();
        var transport = busConfiguration.UseTransport<AzureServiceBusTransport>();
        var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Could not read the 'AzureServiceBus.ConnectionString' environment variable. Check the sample prerequisites.");
        }
        transport.ConnectionString(connectionString);
        busConfiguration.UsePersistence<InMemoryPersistence>();

        using (var bus = Bus.Create(busConfiguration).Start())
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}