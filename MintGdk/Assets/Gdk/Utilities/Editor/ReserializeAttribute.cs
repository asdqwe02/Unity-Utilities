using System;

namespace Mint.Gdk.Utilities.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ReserializeAttribute : Attribute
    {
        // This attribute is used to reserialize the prefab
        // It will be called when the domain is reloaded
        // Delete this attribute when build 
    }
}
