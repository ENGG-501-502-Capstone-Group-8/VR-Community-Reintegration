using UnityEngine;
using UnityEngine.InputSystem;

namespace VRCommunityReintegration.Metrics
{
    /// <summary>
    /// TESTING ONLY. Press keys in Play mode to fake events, so we can check the
    /// counts are right without building a whole route (validation step 1:
    /// "play scripted runs with known mistakes; logged counts must match exactly").
    ///
    /// Uses F-keys because the XR Interaction Simulator already uses number and letter keys.
    ///   F1 Success      F2 Error         F3 Task failure   F4 Rule break
    ///   F5 Backtrack    F6 Hazard missed F7 Interaction    F8 Prompt
    ///   F9 End session  F10 Restart session
    ///
    /// Remove this component (or untick it) before building for the headset.
    /// </summary>
    public class MetricsDebugKeys : MonoBehaviour
    {
        private void Update()
        {
            var kb = Keyboard.current;
            var session = SessionMetrics.Instance;
            if (kb == null || session == null) return;

            if (kb.f1Key.wasPressedThisFrame) session.Record(MetricType.Success, "test key");
            if (kb.f2Key.wasPressedThisFrame) session.Record(MetricType.Error, "test key");
            if (kb.f3Key.wasPressedThisFrame) session.Record(MetricType.TaskFailure, "test key");
            if (kb.f4Key.wasPressedThisFrame) session.Record(MetricType.RuleBreak, "test key");
            if (kb.f5Key.wasPressedThisFrame) session.Record(MetricType.Backtrack, "test key");
            if (kb.f6Key.wasPressedThisFrame) session.Record(MetricType.HazardMissed, "test key");
            if (kb.f7Key.wasPressedThisFrame) session.Record(MetricType.Interaction, "test key");
            if (kb.f8Key.wasPressedThisFrame) session.Record(MetricType.Prompt, "test key");
            if (kb.f9Key.wasPressedThisFrame) session.EndSession();
            if (kb.f10Key.wasPressedThisFrame) session.StartSession();
        }
    }
}
