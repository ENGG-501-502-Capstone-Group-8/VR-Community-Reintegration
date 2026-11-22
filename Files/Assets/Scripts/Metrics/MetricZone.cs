using UnityEngine;

namespace VRCommunityReintegration.Metrics
{
    /// <summary>
    /// An invisible area that records a metric when the player walks into it.
    ///
    /// How to use:
    /// 1. Create an empty GameObject and add this script (a Box Collider is added for you).
    /// 2. Size the box to cover the area (make it at least 3 m tall so the player's head is inside).
    /// 3. Choose the Kind in the Inspector.
    ///
    /// It checks the position of the player's head (the main camera), so it works with
    /// walking, teleporting and the XR simulator without extra physics setup.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class MetricZone : MonoBehaviour
    {
        public enum ZoneKind
        {
            Destination, // the place the patient was asked to go -> Success
            Finish,      // back at the start; ends the session once a destination is reached
            WrongTurn,   // off the correct route -> Error
            RuleBreak,   // e.g. stepping onto the road -> RuleBreak
            Hazard,      // e.g. the rolling ball's path -> HazardMissed
            Backtrack    // walking back the way they came -> Backtrack
        }

        [SerializeField] private ZoneKind kind = ZoneKind.WrongTurn;

        [Tooltip("Shown in the event log, e.g. 'Coffee shop' or 'Crossed on red'.")]
        [SerializeField] private string label = "";

        [Tooltip("Only count the first time the player enters this zone.")]
        [SerializeField] private bool countOnce = false;

        [Tooltip("Leave empty to use the main camera (the player's head).")]
        [SerializeField] private Transform player;

        private BoxCollider zone;
        private bool wasInside;
        private bool hasCounted;

        private void Reset()
        {
            // Make sure the zone never physically blocks the player.
            var box = GetComponent<BoxCollider>();
            if (box != null) box.isTrigger = true;
        }

        private void Awake()
        {
            zone = GetComponent<BoxCollider>();
            zone.isTrigger = true;
        }

        private void Update()
        {
            if (player == null)
            {
                if (Camera.main == null) return;
                player = Camera.main.transform;
            }

            bool isInside = Contains(player.position);
            if (isInside && !wasInside) OnPlayerEntered();
            wasInside = isInside;
        }

        private bool Contains(Vector3 point)
        {
            // Convert the point into the box's own space, then check it against the box size.
            // Works even if the box is rotated or scaled.
            Vector3 local = transform.InverseTransformPoint(point) - zone.center;
            Vector3 half = zone.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x &&
                   Mathf.Abs(local.y) <= half.y &&
                   Mathf.Abs(local.z) <= half.z;
        }

        private void OnPlayerEntered()
        {
            var session = SessionMetrics.Instance;
            if (session == null || !session.IsRunning) return;
            if (countOnce && hasCounted) return;
            hasCounted = true;

            string note = string.IsNullOrEmpty(label) ? name : label;

            switch (kind)
            {
                case ZoneKind.Destination:
                    session.Record(MetricType.Success, $"Reached {note}");
                    break;

                case ZoneKind.Finish:
                    if (session.Count(MetricType.Success) > 0)
                    {
                        session.Record(MetricType.Success, $"Returned to {note}");
                        session.EndSession();
                    }
                    else
                    {
                        hasCounted = false; // not finished yet, so let it count later
                    }
                    break;

                case ZoneKind.WrongTurn:
                    session.Record(MetricType.Error, $"Wrong turn: {note}");
                    break;

                case ZoneKind.RuleBreak:
                    session.Record(MetricType.RuleBreak, note);
                    break;

                case ZoneKind.Hazard:
                    session.Record(MetricType.HazardMissed, note);
                    break;

                case ZoneKind.Backtrack:
                    session.Record(MetricType.Backtrack, note);
                    break;
            }
        }

        // Draws a coloured box in the Scene view so you can see the zones while building.
        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null) return;

            Gizmos.color = kind switch
            {
                ZoneKind.Destination => new Color(0f, 0.8f, 0f, 0.35f),
                ZoneKind.Finish      => new Color(0f, 0.5f, 1f, 0.35f),
                ZoneKind.Hazard      => new Color(1f, 0.6f, 0f, 0.35f),
                _                    => new Color(1f, 0f, 0f, 0.35f)
            };
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}
