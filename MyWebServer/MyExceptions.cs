using System;

namespace MyWebServer
{
    /// <summary>
    /// The exception that is thrown when the server receives a request with "favicon" in its URL.
    /// </summary>
    [Serializable]
    public class FUFaviconException : Exception { }

    /// <summary>
    /// The exception that is thrown when the getter of StatusCode is called and StatusCode has not yet been set.
    /// </summary>
    [Serializable]
    public class StatusCodeNotSetException : Exception { }

    /// <summary>
    /// The exception that is thrown when the server tries to send a response with set content-type but not set content.
    /// </summary>
    [Serializable]
    public class NoContentSetException : Exception { }

    /// <summary>
    /// The exception that is thrown when the PluginManager tries to add an unknown Plugin to its list.
    /// </summary>
    [Serializable]
    public class TriedToAddUnknownPluginException : Exception { }
}