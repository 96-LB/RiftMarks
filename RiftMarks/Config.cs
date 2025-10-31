﻿using BepInEx.Configuration;

namespace RiftMarks;


public static class Config {
    public class Setting<T>(string key, T defaultValue, string description, AcceptableValueBase? acceptableValues = null, object[]? tags = null) {
        private ConfigEntry<T>? entry;
        public ConfigEntry<T> Entry => entry ?? throw new System.InvalidOperationException("Setting is not bound.");

        public static implicit operator T(Setting<T> setting) => setting.Entry.Value;
        public static implicit operator ConfigEntry<T>(Setting<T> setting) => setting.Entry;

        public ConfigEntry<T> Bind(ConfigFile config, string group) {
            return entry = config.Bind(group, key, defaultValue, new ConfigDescription(description, acceptableValues, tags));
        }
    }

    public static class VersionControl {
        const string GROUP = "Version Control";

        public static Setting<string> VersionOverride { get; } = new("Version Override", "", "Input the current build version or '*' to override the version check.");

        public static void Bind(ConfigFile config) {
            VersionOverride.Bind(config, GROUP);
        }
    }

    public static void Bind(ConfigFile config) {
        VersionControl.Bind(config);
    }
}
