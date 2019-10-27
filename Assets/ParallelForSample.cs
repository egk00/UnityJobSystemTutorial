using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.SceneManagement;

public class ParallelForSample : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform sphereTransform;
    private List<SphereT> sphereList;

    public class SphereT
    {
        public Transform transform;
        public float moveY;
    }

    private void Start()
    {
        sphereList = new List<SphereT>();
        for (int i = 0; i < 1000; i++)
        {
            Transform zombieTransform = Instantiate(sphereTransform,
                new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)),
                quaternion.identity);
            sphereList.Add(new SphereT
            {
                transform = zombieTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            });
        }
    }

    private void Update()
    {
        // Time.realtimeSinceStartup : 게임이 시작된 시간으로부터 경과한 실제 시간을 나타냅니다.
        float startTime = Time.realtimeSinceStartup;

        if (useJobs)
        {
            NativeArray<float3> positionArray = new NativeArray<float3>(sphereList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(sphereList.Count, Allocator.TempJob);

            // List 내 위치와 moveY 저장
            for (int i = 0; i < sphereList.Count; i++)
            {
                positionArray[i] = sphereList[i].transform.position;
                moveYArray[i] = sphereList[i].moveY;
            }
            ReallyToughParallelJob reallyToughParallelJob = new ReallyToughParallelJob
            {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = reallyToughParallelJob.Schedule(sphereList.Count, 100);
            // 모든 스레드의 job이 완료될 때 까지 기다림
            jobHandle.Complete();

            for (int i = 0; i < sphereList.Count; i++)
            {
                sphereList[i].transform.position = positionArray[i];
                sphereList[i].moveY = moveYArray[i];
            }

            positionArray.Dispose();
            moveYArray.Dispose();

        }
        else
        {
            foreach (SphereT sphere in sphereList)
            {
                sphere.transform.position += new Vector3(0, sphere.moveY * Time.deltaTime);

                if (sphere.transform.position.y > 5f)
                {
                    sphere.moveY = -math.abs(sphere.moveY);
                }
                if (sphere.transform.position.y < -5f)
                {
                    sphere.moveY = +math.abs(sphere.moveY);
                }
                float value = 0f;
                for (int i = 0; i < 5000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }
        // 1 Update 경과 시간 (ms)
        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }
}

// 병렬 작업
public struct ReallyToughParallelJob : IJobParallelFor
{
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;

    public void Execute(int index)
    {
        positionArray[index] += new float3(0f, moveYArray[index] * deltaTime, 0f);

        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
        float value = 0f;
        for (int i = 0; i < 5000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}