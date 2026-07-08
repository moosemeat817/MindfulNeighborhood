using ModSettings;
using MelonLoader;

namespace MindfulNeighborhood
{
    internal class MindfulNeighborhood : JsonModSettings
    {

        [Name("Enable Mindful Neighborhood")]
        [Description("Adds several structures to the Mindful Cabin area and replaces the potbelly stove with a fireplace.")]
        public bool mindfulNeighborhood = false;

        [Name("Move Justy's Hovel Cabin to Mindful Cabin area")]
        [Description("Move's Justy's Hovel cabin to the Mindful Cabin area")]
        public bool moveJustys = false;


        [Name("Debug Mode")]
        [Description("Enables verbose logging for troubleshooting.")]
        public bool debugMode = false;

    }

    internal static class Settings
    {
        public static MindfulNeighborhood options;

        public static void OnLoad()
        {
            options = new MindfulNeighborhood();
            options.AddToModSettings("Mindful Neighborhood", MenuType.Both);
        }
    }

   
    internal static class Log
    {
        public static void Msg(string message)
        {
            if (Settings.options != null && Settings.options.debugMode)
                MelonLogger.Msg(message);
        }

        public static void Warning(string message)
        {
            if (Settings.options != null && Settings.options.debugMode)
                MelonLogger.Warning(message);
        }

        public static void Error(string message)
        {
            if (Settings.options != null && Settings.options.debugMode)
                MelonLogger.Error(message);
        }
    }

}