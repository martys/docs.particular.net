﻿using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Routing.Legacy;

class Program
{
    static void Main()
    {

        #region WorkerIdentity

        var endpointConfiguration = new EndpointConfiguration("Samples.Scaleout.Worker");
        var scaleOut = endpointConfiguration.ScaleOut();
        scaleOut.InstanceDiscriminator(ConfigurationManager.AppSettings["InstanceId"]);

        #endregion

        #region Enlisting

        endpointConfiguration.EnlistWithLegacyMSMQDistributor(
            masterNodeAddress: ConfigurationManager.AppSettings["DistributorAddress"],
            masterNodeControlAddress: ConfigurationManager.AppSettings["DistributorControlAddress"],
            capacity: 10);

        #endregion

        endpointConfiguration.UseSerialization<JsonSerializer>();
        endpointConfiguration.UsePersistence<InMemoryPersistence>();
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.EnableInstallers();
        var conventions = endpointConfiguration.Conventions();
        conventions.DefiningMessagesAs(t => t.GetInterfaces().Contains(typeof (IMessage)));

        Run(endpointConfiguration).GetAwaiter().GetResult();
    }

    static async Task Run(EndpointConfiguration busConfiguration)
    {
        var endpointInstance = await Endpoint.Start(busConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}