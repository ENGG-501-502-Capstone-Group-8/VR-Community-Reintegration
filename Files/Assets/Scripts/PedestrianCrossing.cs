using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public sealed class PedestrianCrossing : MonoBehaviour
{
    // Main Hub ground mesh: zebra stripes x = +/-1.95, asphalt z = 1.9 to 9.5.
    public const float HalfWidth = 1.95f;
    private const float South = 1.9f;
    private const float North = 9.5f;
    [SerializeField] private Transform headset;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    public bool Active { get; private set; }
    private readonly Transform[] buttons = new Transform[2];
    private readonly GameObject[] lamps = new GameObject[2];
    private InputAction leftPress, rightPress;
    private bool crossing, deviated, outside, sampled;
    private int origin;
    private Vector3 previous;
    private double started;
    private float flashStart;

    private void Awake()
    {
        BuildPole(0, new Vector3(-2.18f, 0, South - 0.45f), 180);
        BuildPole(1, new Vector3(2.18f, 0, North + 0.45f), 0);
        leftPress = new InputAction("Left crossing button", InputActionType.Button, "<XRController>{LeftHand}/triggerPressed");
        rightPress = new InputAction("Right crossing button", InputActionType.Button, "<XRController>{RightHand}/triggerPressed");
    }

    private void OnEnable()
    {
        leftPress.Enable();
        rightPress.Enable();
        sampled = crossing = deviated = Active = false;
    }

    private void OnDisable()
    {
        leftPress.Disable();
        rightPress.Disable();
        foreach (var lamp in lamps) lamp.SetActive(false);
    }

    private void OnDestroy()
    {
        leftPress.Dispose();
        rightPress.Dispose();
    }

    private void Update()
    {
        Vector3 point = transform.InverseTransformPoint(headset.position);
        if (!Active)
        {
            foreach (Transform button in buttons)
            {
                bool pressed = NearPress(leftHand, leftPress, button) || NearPress(rightHand, rightPress, button);
#if UNITY_EDITOR
                Vector3 delta = headset.position - button.position;
                pressed |= Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame
                    && new Vector2(delta.x, delta.z).magnitude <= 1.2f;
#endif
                if (!pressed) continue;
                Active = true;
                flashStart = Time.time;
                Debug.Log("[Crosswalk] Button pressed. Traffic yielding.");
                break;
            }
        }

        Track(point);
        bool lit = Active && Mathf.Repeat(Time.time - flashStart, 1f) < 0.5f;
        foreach (var lamp in lamps)
            if (lamp.activeSelf != lit) lamp.SetActive(lit);
        previous = point;
        sampled = true;
    }

    private static bool NearPress(Transform hand, InputAction action, Transform button)
    {
        return action.WasPressedThisFrame() && Vector3.Distance(hand.position, button.position) <= 0.45f;
    }

    private static int Bank(float z) => z <= South ? -1 : z >= North ? 1 : 0;
    private static float EdgeX(Vector3 from, Vector3 to, int bank)
    {
        float t = Mathf.InverseLerp(from.z, to.z, bank < 0 ? South : North);
        return Mathf.Lerp(from.x, to.x, t);
    }

    private void Track(Vector3 point)
    {
        if (!sampled) return;
        int bank = Bank(point.z);
        int lastBank = Bank(previous.z);
        if (!crossing)
        {
            if (lastBank != 0 && lastBank == bank) return;
            if (lastBank == 0)
            {
                bool enteredFromSide = bank == 0 && Mathf.Abs(previous.x) > HalfWidth && Mathf.Abs(point.x) <= HalfWidth;
                if (!enteredFromSide) return;
            }
            bool sideEntry = lastBank == 0;
            bool wide = !sideEntry && Mathf.Abs(EdgeX(previous, point, lastBank)) > HalfWidth;
            if (wide && !Active) return;
            origin = lastBank;
            crossing = true;
            deviated = outside = wide || sideEntry;
            started = Time.timeAsDouble;
            Debug.Log("[Crosswalk] Entered roadway. Signal " + (Active ? "active." : "idle."));
            if (deviated) Debug.LogWarning("[Crosswalk] Entered outside the marked crossing.");
        }

        float x = bank == 0 ? point.x : EdgeX(previous, point, bank);
        bool beyond = Mathf.Abs(x) > HalfWidth;
        if (beyond && !outside)
        {
            deviated = true;
            Debug.LogWarning("[Crosswalk] Exited the side of the marked crossing while in the street.");
        }
        outside = beyond;
        if (bank == 0) return;

        string outcome = bank == origin ? "Returned to starting sidewalk"
            : deviated ? "Reached sidewalk after leaving the marked route"
            : "Exited safely to opposite sidewalk";
        Debug.Log($"[Crosswalk] {outcome}. Total time: {Time.timeAsDouble - started:F2} s.");
        crossing = Active = false;
    }

    private void BuildPole(int index, Vector3 position, float yaw)
    {
        var root = new GameObject(index == 0 ? "South crossing pole" : "North crossing pole").transform;
        root.SetParent(transform, false);
        root.localPosition = position;
        root.localRotation = Quaternion.Euler(0, yaw, 0);
        Color dark = new Color(0.12f, 0.14f, 0.16f);
        Color yellow = new Color(1f, 0.72f, 0.06f);
        Part(root, "Post", new Vector3(0, 1.35f, 0), new Vector3(.09f, 2.7f, .09f), Color.gray);
        Part(root, "Pedestrian sign", new Vector3(0, 2.1f, 0), new Vector3(.58f, .62f, .08f), yellow);
        // Walking-person symbol on both faces.
        foreach (int face in new[] { -1, 1 })
        {
            float z = face * .051f;
            Part(root, "Head", new Vector3(.05f, 2.29f, z), Vector3.one * .09f, dark, 0, PrimitiveType.Sphere);
            Part(root, "Body", new Vector3(0, 2.14f, z), new Vector3(.07f,.19f,.02f), dark, -16);
            Part(root, "Arm", new Vector3(-.07f,2.16f,z), new Vector3(.17f,.04f,.02f), dark, 33);
            Part(root, "Arm", new Vector3(.08f,2.13f,z), new Vector3(.15f,.04f,.02f), dark, -35);
            Part(root, "Leg", new Vector3(-.07f,1.99f,z), new Vector3(.055f,.19f,.02f), dark, -34);
            Part(root, "Leg", new Vector3(.06f,1.99f,z), new Vector3(.055f,.19f,.02f), dark, 36);
        }
        Part(root, "Beacon housing", new Vector3(0,2.57f,0), Vector3.one * .22f, dark, 0, PrimitiveType.Sphere);
        lamps[index] = Part(root, "Amber beacon", new Vector3(0,2.57f,0), Vector3.one * .24f, yellow, 0, PrimitiveType.Sphere).gameObject;
        lamps[index].SetActive(false);
        Part(root, "Button housing", new Vector3(0,1.05f,.07f), new Vector3(.2f,.30f,.14f), dark);
        buttons[index] = Part(root, "Push button", new Vector3(0,1.05f,.15f), new Vector3(.14f,.14f,.06f), yellow, 0, PrimitiveType.Sphere);
    }

    private static Transform Part(Transform parent, string label, Vector3 pos, Vector3 size,
        Color color, float angle = 0, PrimitiveType shape = PrimitiveType.Cube)
    {
        GameObject obj = GameObject.CreatePrimitive(shape);
        obj.name = label;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = pos;
        obj.transform.localRotation = Quaternion.Euler(0, 0, angle);
        obj.transform.localScale = size;
        Destroy(obj.GetComponent<Collider>());
        var props = new MaterialPropertyBlock();
        props.SetColor("_BaseColor", color);
        props.SetColor("_Color", color);
        obj.GetComponent<Renderer>().SetPropertyBlock(props);
        return obj.transform;
    }
}
