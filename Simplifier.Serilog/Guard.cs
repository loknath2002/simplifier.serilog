using System;
namespace Simplifier.Serilog
{
    public static class Guard
    {
        internal static void AgainstNull(object theObj, string theParamName)
        {
            if (theObj == null)
            {
                throw new ArgumentException($"{theParamName} must not be null");
            }
        }

        internal static void AgainstNullOrWhiteSpace(string theVariable, string theVarName)
        {
            if (string.IsNullOrWhiteSpace(theVariable))
            {
                throw new ArgumentException($"{theVarName} must be non-null or empty", theVarName);
            }
        }
    }
}