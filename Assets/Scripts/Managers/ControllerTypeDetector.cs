using UnityEngine;
using UnityEngine.InputSystem;

public static class ControllerTypeDetector
{
    public enum ControllerType { KeyboardMouse, Xbox, PlayStation, Other }
    public static ControllerType CurrentType = ControllerType.KeyboardMouse;

    public static ControllerType Detect()
    {
        foreach (var device in InputSystem.devices)
        {
            string name = device.displayName ?? device.name;

            // Xbox check
            if (name.Contains("Xbox") || name.Contains("XInput"))
                return CurrentType = ControllerType.Xbox;

            // PlayStation check (DualShock / DualSense / Wireless Controller)
            if (name.Contains("DualShock") ||
                name.Contains("DualSense") ||
                name.Contains("Wireless Controller") ||
                name.Contains("PS"))
                return CurrentType = ControllerType.PlayStation;
        }

        return CurrentType = ControllerType.KeyboardMouse;
    }
}
