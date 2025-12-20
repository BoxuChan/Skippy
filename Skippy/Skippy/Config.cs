using System;
using Dalamud.Configuration;

namespace Plugins.a08381.Skippy
{
    [Serializable]
    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public bool IsEnabled { get; set; } = true;
    }
}