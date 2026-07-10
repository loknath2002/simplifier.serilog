using System;
namespace Simplifier.Serilog
{
    /// <summary>
    /// Guard class for parameter validation.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Guards against null objects.
        /// </summary>
        /// <param name="theObj">The object to check.</param>
        /// <param name="theParamName">The parameter name.</param>
        internal static void AgainstNull(object theObj, string theParamName)
        {
            if (theObj == null)
            {
                throw new ArgumentException($"{theParamName} must not be null");
            }
        }

        /// <summary>
        /// Guards against null or white space strings.
        /// </summary>
        /// <param name="theVariable">The string to check.</param>
        /// <param name="theVarName">The variable name.</param>
        internal static void AgainstNullOrWhiteSpace(string theVariable, string theVarName)
        {
            if (string.IsNullOrWhiteSpace(theVariable))
            {
                throw new ArgumentException($"{theVarName} must be non-null or empty", theVarName);
            }
        }
    }
}