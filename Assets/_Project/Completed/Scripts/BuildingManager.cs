using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace InfallibleCode.Completed
{
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField] private List<Building> buildings;
        
        private BuildingUpdateJob _job;
        private NativeArray<Building.Data> _buildingDataArray;

        private void Awake()
        {
            var buildingData = new Building.Data[buildings.Count];
            for (var i = 0; i < buildingData.Length; i++)
            {
                buildingData[i] = new Building.Data(buildings[i]);
            }
            
            _buildingDataArray = new NativeArray<Building.Data>(buildingData, Allocator.Persistent);
            
            _job = new BuildingUpdateJob
            {
                BuildingDataArray = _buildingDataArray
            };
        }

        private void Update()
        {
            var jobHandle = _job.Schedule(buildings.Count, 1);
            jobHandle.Complete();
        }

        private void OnDestroy()
        {
            _buildingDataArray.Dispose();
        }
    }
}