using System;

namespace App.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class ExcludeFromCoverage : Attribute
    {
        // Just for show
    }
}
