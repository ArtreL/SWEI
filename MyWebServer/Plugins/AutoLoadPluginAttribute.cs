using System;

namespace MyWebServer.Plugins
{
    /// <summary>
    /// Attribute to ensure correct loading properties for plugins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoLoadPluginAttribute : Attribute
    {
    }
}
