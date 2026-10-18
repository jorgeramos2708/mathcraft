using UnityEngine;

namespace VoxelSandbox.World
{
    public class DayNightCycle : MonoBehaviour
    {
        public float DayLength = 120f;
        private float _time;

        public bool IsDay => (_time % DayLength) < DayLength * 0.6f;

        void Update()
        {
            _time += Time.deltaTime;
        }

        public float GetTimeOfDay()
        {
            return _time % DayLength / DayLength;
        }
    }
}
