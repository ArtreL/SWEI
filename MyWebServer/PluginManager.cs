using BIF.SWE1.Interfaces;
using MyWebServer.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MyWebServer
{
    /// <summary>
    /// Implements a PluginManager object, that contains all available Plugins.
    /// </summary>
    public class PluginManager : IPluginManager
    {
        /// <summary>
        /// Initializes the List of Plugins by collecting Plugins from the project and given .dll files.
        /// </summary>
        public PluginManager()
        {
            string wdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var lst = Directory.GetFiles(wdir)
                .Where(i => new[] { ".dll", ".exe" }.Contains(Path.GetExtension(i)))
                .SelectMany(i => Assembly.LoadFrom(i).GetTypes())
                .Where(myType => myType.IsClass 
                              && !myType.IsAbstract 
                              && myType.GetCustomAttributes(true).Any(i => i.GetType() == typeof(AutoLoadPluginAttribute))
                              && myType.GetInterfaces().Any(i => i == typeof(IPlugin)));

            foreach (Type type in lst)
            {
                AllPlugins.Add((IPlugin)Activator.CreateInstance(type));
            }
        }

        private List<IPlugin> AllPlugins = new List<IPlugin>();
        private Mutex PlugMut = new Mutex();

        /// <summary>
        /// Returns a list of all plugins. Never returns null.
        /// </summary>
        public IEnumerable<IPlugin> Plugins => AllPlugins;

        /// <summary>
        /// Adds a new plugin. If the plugin was already added, nothing happens.
        /// </summary>
        /// <param name="plugin">
        /// The Plugin that is supposed to be added to the List of Plugins.
        /// </param>
        public void Add(IPlugin plugin)
        {
            if (!AllPlugins.Any(x => x == plugin))
            {
                PlugMut.WaitOne();
                AllPlugins.Add(plugin);
                PlugMut.ReleaseMutex();
            }
        }

        /// <summary>
        /// Adds a new plugin by type name. If the plugin was already added, nothing happens.
        /// Throws an exception, when the type cannot be resolved or the type does not implement IPlugin.
        /// </summary>
        /// <param name="plugin">
        /// The Plugin that is supposed to be added to the List of Plugins represented by its name as a string.
        /// </param>
        public void Add(string plugin)
        {
            if(Type.GetType(plugin) != null)
            {
                IPlugin AddThis = (IPlugin)Activator.CreateInstance(Type.GetType(plugin, throwOnError: true));

                if(!AllPlugins.Any(x => x.GetType() == AddThis.GetType()))
                {
                    PlugMut.WaitOne();
                    AllPlugins.Add(AddThis);
                    PlugMut.ReleaseMutex();
                }
            }
            else
            {
                throw new TriedToAddUnknownPluginException();
            }
        }

        /// <summary>
        /// Clears all plugins.
        /// </summary>
        public void Clear()
        {
            AllPlugins.Clear();
        }
        
        /// <summary>
        /// Fetches the Plugin with the highest CanHandle value.
        /// </summary>
        /// <param name="req">
        /// The Request that is supposed to be handled.
        /// </param>
        /// <returns>
        /// Returns the Plugin that delivered the highest CanHandle value. Returns the last Plugin in the List if tied.
        /// </returns>
        public IPlugin GetPlugin(Request req)
        {
            PlugMut.WaitOne();
            IPlugin HandlePlugin = AllPlugins.Select(i => new { Value = i.CanHandle(req), Plugin = i }).OrderBy(i => i.Value).Last().Plugin;
            PlugMut.ReleaseMutex();
            return HandlePlugin;
        }

        /// <summary>
        /// Updates the list of plugins to see if any have been added.
        /// </summary>
        public void UpdatePlugins()
        {
            string wdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var lst = Directory.GetFiles(wdir)
                .Where(i => new[] { ".dll", ".exe" }.Contains(Path.GetExtension(i)))
                .SelectMany(i => Assembly.LoadFrom(i).GetTypes())
                .Where(myType => myType.IsClass
                              && !myType.IsAbstract
                              && myType.GetCustomAttributes(true).Any(i => i.GetType() == typeof(AutoLoadPluginAttribute))
                              && myType.GetInterfaces().Any(i => i == typeof(IPlugin)));

            foreach (Type type in lst)
            {
                PlugMut.WaitOne();
                if (!AllPlugins.Contains((IPlugin)Activator.CreateInstance(type)))
                {
                    AllPlugins.Add((IPlugin)Activator.CreateInstance(type));
                }
                PlugMut.ReleaseMutex();
            }
        }
    }
}
