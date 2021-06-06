using System;
using System.Reflection;
using Models;
using WHO.Tracking;

namespace WHO.Extensions
{
    public static class InfectionTotalsExtensions
    {
        public static long GetParameterTotals(this InfectionTotals totals, TrackingValue value)
        {
            Type myType = typeof(InfectionTotals);
            PropertyInfo myPropInfo = myType.GetProperty(value.ToString()) ?? throw new InvalidOperationException("Attempting to retrieve property which doesn't exist");
            var propertyValue = myPropInfo.GetValue(totals, null) ?? throw new InvalidOperationException("Retrieved value is null");
            return (long)propertyValue;
        }
    }
}
