using System;

namespace ED47.Stack.Reflector
{
    /// <summary>
    /// Contains a controller's kind and Type.
    /// </summary>
    internal class ControllerItem
    {
        public ControllerKind Kind { get; set; }
        public Type Type { get; set; }
    }

    public enum ControllerKind
    {
        Mvc,
        Api
    }
}
