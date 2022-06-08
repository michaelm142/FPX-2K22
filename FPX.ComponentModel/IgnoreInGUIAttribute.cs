using System;

namespace FPX.Editor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreInGUIAttribute : Attribute
    {

    }
}