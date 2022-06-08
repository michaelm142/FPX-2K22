using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPX
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorAttribute : Attribute
    {
        public Type EditorType { get; private set; }

        public EditorAttribute(Type EditorType)
        {
            this.EditorType = EditorType;
        }
    }
}
