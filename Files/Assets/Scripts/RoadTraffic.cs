using System.Collections.Generic;
using UnityEngine;

public sealed class RoadTraffic : MonoBehaviour
{
    [SerializeField] private Mesh[] carModels;
    [SerializeField] private Material surface;
    [SerializeField] private PedestrianCrossing crossing;

    private const float RoadEnd = 34f;
    private const float HalfLength = 2.225f;
    private const float Spacing = 2 * HalfLength + 1.5f;
    private const float CruiseSpeed = 6f;

    private sealed class Car
    {
        public MeshFilter model;
        public float position; // Distance along the lane, with zero at the crossing.
        public float speed;
    }

    private sealed class Lane
    {
        public readonly List<Car> cars = new List<Car>();
        public float nextArrival;
    }

    private readonly Lane[] lanes = { new Lane(), new Lane() };
    private readonly Stack<Car> parked = new Stack<Car>();

    private void OnEnable()
    {
        foreach (Lane lane in lanes)
            lane.nextArrival = Time.time + Random.Range(0.2f, 2f);
    }

    private void OnDisable()
    {
        foreach (Lane lane in lanes)
        {
            foreach (Car car in lane.cars)
                Recycle(car);
            lane.cars.Clear();
        }
    }

    private void Recycle(Car car)
    {
        car.model.gameObject.SetActive(false);
        parked.Push(car);
    }

    private void Update()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            Lane lane = lanes[i];
            float direction = i == 0 ? 1f : -1f;
            float limit = float.PositiveInfinity;
            for (int j = 0; j < lane.cars.Count; j++)
            {
                Car car = lane.cars[j];
                float front = car.position + HalfLength;
                if (crossing.Active && front <= -PedestrianCrossing.HalfWidth)
                {
                    // A late request may arrive inside the usual stopping margin.
                    float stop = -PedestrianCrossing.HalfWidth - HalfLength - 0.6f;
                    limit = Mathf.Min(limit, Mathf.Max(car.position, stop));
                }

                float room = Mathf.Max(0f, limit - car.position);
                float target = Mathf.Min(CruiseSpeed, Mathf.Sqrt(2f * 4f * room));
                car.speed = Mathf.MoveTowards(car.speed, target,
                    (target < car.speed ? 4f : 2.5f) * Time.deltaTime);
                float step = Mathf.Min(room, car.speed * Time.deltaTime);
                car.position += step;
                if (step >= room) car.speed = 0f;
                car.model.transform.localPosition = new Vector3(direction * car.position, 0, i == 0 ? 4f : 7.5f);
                limit = car.position - Spacing;
            }

            if (lane.cars.Count > 0 && lane.cars[0].position >= RoadEnd)
            {
                Recycle(lane.cars[0]);
                lane.cars.RemoveAt(0);
            }

            if (Time.time >= lane.nextArrival && limit >= -RoadEnd)
            {
                Spawn(lane, direction);
                lane.nextArrival = Time.time + Random.Range(2.2f, 4.8f);
            }
        }
    }

    private void Spawn(Lane lane, float direction)
    {
        Car car;
        if (parked.Count > 0)
            car = parked.Pop();
        else
        {
            var obj = new GameObject("Traffic car");
            obj.transform.SetParent(transform, false);
            car = new Car { model = obj.AddComponent<MeshFilter>() };
            obj.AddComponent<MeshRenderer>().sharedMaterial = surface;
        }
        car.position = -RoadEnd;
        car.speed = CruiseSpeed;
        car.model.sharedMesh = carModels[Random.Range(0, carModels.Length)];
        car.model.transform.localPosition = new Vector3(-direction * RoadEnd, 0, direction > 0 ? 4f : 7.5f);
        car.model.transform.localRotation = Quaternion.Euler(0, direction > 0 ? 0 : 180, 0);
        car.model.gameObject.SetActive(true);
        lane.cars.Add(car);
    }
}
