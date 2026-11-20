using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRCommunityReintegration.Metrics
{
    /// <summary>
    /// The kinds of things we count during a session.
    /// Based on the metrics the sponsor already tracks (Sept 22 meeting),
    /// plus "Success" for encouraging feedback (site visit).
    /// </summary>
    public enum MetricType
    {
        Success,        // reached a destination / completed a step
        Error,          // general mistake (e.g. wrong turn)
        TaskFailure,    // gave up on, or failed, a task
        Interaction,    // used an object, read a sign, pressed a button
        RuleBreak,      // broke a task rule (e.g. stepped onto the road)
        Backtrack,      // turned back along the route
        HazardMissed,   // walked into a hazard instead of avoiding it
        Prompt          // clinician had to give a hint
    }

    /// <summary>One thing that happened during a session, and when.</summary>
    public readonly struct MetricEvent
    {
        public readonly float Seconds;
        public readonly MetricType Type;
        public readonly string Note;

        public MetricEvent(float seconds, MetricType type, string note)
        {
            Seconds = seconds;
            Type = type;
            Note = note;
        }

        public override string ToString()
        {
            string time = TimeSpan.FromSeconds(Seconds).ToString(@"mm\:ss");
            return string.IsNullOrEmpty(Note) ? $"{time}  {Type}" : $"{time}  {Type}: {Note}";
        }
    }

    /// <summary>
    /// Keeps track of one session: the timer, the counts, and a list of events.
    /// Put ONE of these in the scene (e.g. on an empty GameObject called "Session").
    /// Nothing is saved to disk: results live in memory only (sponsor: no stored personal data).
    /// </summary>
    public class SessionMetrics : MonoBehaviour
    {
        /// <summary>Lets any other script reach the session with SessionMetrics.Instance.</summary>
        public static SessionMetrics Instance { get; private set; }

        [Tooltip("Start the timer automatically when Play is pressed.")]
        [SerializeField] private bool startOnPlay = true;

        public bool IsRunning { get; private set; }
        public bool HasEnded { get; private set; }

        /// <summary>Seconds since the session started (frozen once it ends).</summary>
        public float ElapsedSeconds => IsRunning ? Time.time - startTime : endTime - startTime;

        public IReadOnlyList<MetricEvent> Events => events;

        /// <summary>Other scripts can listen to these (e.g. the results board).</summary>
        public event Action<MetricEvent> EventRecorded;
        public event Action SessionEnded;

        private readonly Dictionary<MetricType, int> counts = new Dictionary<MetricType, int>();
        private readonly List<MetricEvent> events = new List<MetricEvent>();
        private float startTime;
        private float endTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[Metrics] More than one SessionMetrics in the scene. Removing the extra one.");
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (startOnPlay) StartSession();
        }

        /// <summary>Clears everything and starts the timer.</summary>
        public void StartSession()
        {
            counts.Clear();
            events.Clear();
            startTime = Time.time;
            endTime = startTime;
            IsRunning = true;
            HasEnded = false;
            Debug.Log("[Metrics] Session started");
        }

        /// <summary>Stops the timer and prints a summary to the Console.</summary>
        public void EndSession()
        {
            if (!IsRunning) return;
            endTime = Time.time;
            IsRunning = false;
            HasEnded = true;
            Debug.Log(BuildSummary());
            SessionEnded?.Invoke();
        }

        /// <summary>Count one event. Call this from any other script.</summary>
        public void Record(MetricType type, string note = "")
        {
            if (!IsRunning) return;

            counts[type] = Count(type) + 1;
            var e = new MetricEvent(ElapsedSeconds, type, note);
            events.Add(e);
            Debug.Log($"[Metrics] {e}");
            EventRecorded?.Invoke(e);
        }

        /// <summary>How many times this type of event has happened.</summary>
        public int Count(MetricType type)
        {
            return counts.TryGetValue(type, out int n) ? n : 0;
        }

        /// <summary>
        /// Simple total of the "mistake" types. The clinician interprets this;
        /// the system does not decide readiness.
        /// </summary>
        public int TotalErrorScore =>
            Count(MetricType.Error) + Count(MetricType.TaskFailure) +
            Count(MetricType.RuleBreak) + Count(MetricType.HazardMissed);

        /// <summary>A plain-text summary for the Console or the results board.</summary>
        public string BuildSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine(HasEnded ? "Session complete" : "Session in progress");
            sb.AppendLine($"Time: {TimeSpan.FromSeconds(ElapsedSeconds):mm\\:ss}");
            sb.AppendLine($"Successes: {Count(MetricType.Success)}");
            sb.AppendLine($"Errors: {Count(MetricType.Error)}");
            sb.AppendLine($"Task failures: {Count(MetricType.TaskFailure)}");
            sb.AppendLine($"Rule breaks: {Count(MetricType.RuleBreak)}");
            sb.AppendLine($"Backtracking: {Count(MetricType.Backtrack)}");
            sb.AppendLine($"Hazards missed: {Count(MetricType.HazardMissed)}");
            sb.AppendLine($"Interactions: {Count(MetricType.Interaction)}");
            sb.AppendLine($"Prompts: {Count(MetricType.Prompt)}");
            sb.Append($"Total error score: {TotalErrorScore}");
            return sb.ToString();
        }
    }
}
