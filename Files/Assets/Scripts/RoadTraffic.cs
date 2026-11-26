using UnityEngine;

public sealed class RoadTraffic : MonoBehaviour
{
    [SerializeField] private Mesh[] carModels;
    [SerializeField] private Material surface;

    private const float Window = 10f;
    private const float RoadEnd = 34f;
    private const float Speed = 6f;

    // A crossing takes under 12 seconds and can span at most three windows.
    private readonly MeshFilter[] cars = new MeshFilter[3];
    private float windowStart;
    private float nextSpawn;
    private float elapsed;

    private void Awake()
    {
        for (int i = 0; i < cars.Length; i++)
        {
            var car = new GameObject("Traffic car");
            car.transform.SetParent(transform, false);
            cars[i] = car.AddComponent<MeshFilter>();
            car.AddComponent<MeshRenderer>().sharedMaterial = surface;
            car.SetActive(false);
        }
    }

    private void OnEnable()
    {
        elapsed = 0f;
        windowStart = 0f;
        ScheduleSpawn();
    }

    private void OnDisable()
    {
        foreach (var car in cars)
            car.gameObject.SetActive(false);
    }

    private void ScheduleSpawn()
    {
        // The one-second margins keep consecutive cars in the same lane apart.
        nextSpawn = windowStart + Random.Range(1f, Window - 1f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        foreach (var car in cars)
        {
            if (!car.gameObject.activeSelf)
                continue;

            car.transform.Translate(Vector3.right * (Speed * Time.deltaTime), Space.Self);
            if (Mathf.Abs(car.transform.localPosition.x) >= RoadEnd)
                car.gameObject.SetActive(false);
        }

        if (elapsed >= nextSpawn)
        {
            SpawnCar(elapsed - nextSpawn);
            windowStart += Window;
            ScheduleSpawn();
        }
    }

    private void SpawnCar(float age)
    {
        bool eastbound = Random.value < 0.5f;
        foreach (var car in cars)
        {
            if (car.gameObject.activeSelf)
                continue;

            car.sharedMesh = carModels[Random.Range(0, carModels.Length)];
            float x = -RoadEnd + Speed * age;
            car.transform.localPosition = new Vector3(eastbound ? x : -x, 0f, eastbound ? 4f : 7.5f);
            car.transform.localRotation = Quaternion.Euler(0f, eastbound ? 0f : 180f, 0f);
            car.gameObject.SetActive(true);
            return;
        }
    }
}
