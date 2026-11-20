using System;
using TMPro;
using UnityEngine;

namespace VRCommunityReintegration.Metrics
{
    /// <summary>
    /// Shows the current time (hh:mm, requested at the site visit) and the session results
    /// on TextMeshPro text in the scene.
    ///
    /// How to use:
    /// 1. GameObject > 3D Object > Text - TextMeshPro (or UI text on a world-space canvas).
    /// 2. Add this script to any object and drag the text objects into the slots.
    /// </summary>
    public class ResultsBoard : MonoBehaviour
    {
        [Tooltip("Optional: shows the current time, e.g. 14:05.")]
        [SerializeField] private TMP_Text clockText;

        [Tooltip("Shows the counts and time.")]
        [SerializeField] private TMP_Text resultsText;

        [Tooltip("Show counts while the session is running. Turn off to show results only at the end.")]
        [SerializeField] private bool showDuringSession = true;

        [Tooltip("Clinician setting: whether the patient can see the results in the headset.")]
        [SerializeField] private bool showToPatient = true;

        private void Update()
        {
            if (clockText != null)
            {
                clockText.text = DateTime.Now.ToString("HH:mm");
            }

            if (resultsText == null) return;

            var session = SessionMetrics.Instance;
            bool visible = showToPatient && session != null &&
                           (session.HasEnded || (showDuringSession && session.IsRunning));

            resultsText.text = visible ? session.BuildSummary() : "";
        }

        /// <summary>Can be hooked up to a button later for the clinician.</summary>
        public void SetShowToPatient(bool show)
        {
            showToPatient = show;
        }
    }
}
