using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
// IJobParallelForTransform을 구현하기 위해 선언
using UnityEngine.Jobs;

public class ParallelForTransformSample : MonoBehaviour
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
        for(int i=0; i<1000; i++)
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
            NativeArray<float> moveYArray = new NativeArray<float>(sphereList.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(sphereList.Count);

            // List 내 위치와 moveY 저장
            for(int i=0; i < sphereList.Count; i++)
            {
                //positionArray[i] = sphereList[i].transform.position;
                moveYArray[i] = sphereList[i].moveY;
                transformAccessArray.Add(sphereList[i].transform);
                
            }

            ReallyToughParalleJobTransform reallyToughParalleJobTransform = new ReallyToughParalleJobTransform
            {
                deltaTime = Time.deltaTime,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = reallyToughParalleJobTransform.Schedule(transformAccessArray);
            jobHandle.Complete();

            for(int i =0; i < sphereList.Count; i++)
            {
                sphereList[i].moveY = moveYArray[i];
            }
            
            moveYArray.Dispose();
            transformAccessArray.Dispose();
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

[BurstCompile]
public struct ReallyToughParalleJobTransform : IJobParallelForTransform
{
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(0f, moveYArray[index] * deltaTime, 0f);

        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f)
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

