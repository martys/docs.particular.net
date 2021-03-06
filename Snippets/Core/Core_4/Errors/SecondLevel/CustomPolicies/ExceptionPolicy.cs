﻿namespace Core4.Errors.SecondLevel.CustomPolicies
{
    using System;
    using NServiceBus;

    class ExceptionPolicy
    {

        ExceptionPolicy()
        {
            Configure.Features.SecondLevelRetries(s => s.CustomRetryPolicy(MyCustomRetryPolicy));
        }

        #region SecondLevelRetriesCustomExceptionPolicyHandler

        TimeSpan MyCustomRetryPolicy(TransportMessage transportMessage)
        {
            if (transportMessage.ExceptionType() == typeof(MyBusinessException).FullName)
            {
                // Do not retry for MyBusinessException
                return TimeSpan.MinValue;
            }

            if (transportMessage.NumberOfRetries() >= 3)
            {
                return TimeSpan.MinValue;
            }

            return TimeSpan.FromSeconds(5);
        }

        #endregion
    }

    class MyBusinessException :
        Exception
    {
    }
}