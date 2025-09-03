using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Kittyn.Tools
{
    public static class KittynLocalizationDebug
    {
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🧪 QA/Log Localization Status", false, 801)]
        public static void LogStatus()
        {
            // Force init
            var langs = KittynLocalization.AvailableLanguages;
            Debug.Log($"[Localization] Current: {KittynLocalization.CurrentLanguage} | Available: {string.Join(", ", langs)}");

            // Sample keys
            string[] sample = new[]
            {
                "common.language",
                "comfi_hierarchy.window_title",
                "enhanced_dynamics.status",
                "immersive_scaler.window_title",
            };

            foreach (var code in langs.OrderBy(s => s))
            {
                var ok = sample.Select(k => (k, KittynLocalization.Get(k, code))).ToArray();
                Debug.Log($"[Localization] {code}: " + string.Join(" | ", ok.Select(p => $"{p.k}='{p.Item2}'")));
            }
        }
    }
}

