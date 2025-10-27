using Meryel.UnityCodeAssist.Editor.Logger;
using UnityEditor;
#pragma warning disable IDE0005

#pragma warning restore IDE0005


#nullable enable


namespace Meryel.UnityCodeAssist.Editor
{
    [InitializeOnLoad]
    public static class LazyInitializer
    {
        private static int counter;

        static LazyInitializer()
        {
            counter = -5; // start initializing five frames later
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            counter++;

            if (counter == 1)
                MainThreadDispatcher.Bump();
            else if (counter == 2)
                ELogger.Bump();
            else if (counter == 3)
                Monitor.Bump();
            else if (counter == 4)
                MQTTnetInitializer.Bump();
            else if (counter == 5)
                Updater.CheckUpdateSilent();
            else if (counter >= 6)
                EditorApplication.update -= OnUpdate;
        }
    }
}