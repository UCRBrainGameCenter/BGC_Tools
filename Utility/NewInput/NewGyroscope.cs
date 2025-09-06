using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ISGyroscope = UnityEngine.InputSystem.Gyroscope;


namespace BGC.Utility
{
    public class NewGyroscope
    {
#if ENABLE_INPUT_SYSTEM
        private ISGyroscope _gyro => ISGyroscope.current;
        private Accelerometer _accel => Accelerometer.current;
        private GravitySensor _gravity => GravitySensor.current;
        private AttitudeSensor _attitude => AttitudeSensor.current;
#endif

        public bool enabled { get; set; }

        /// <summary>Time interval between updates (in seconds).</summary>
        public float updateInterval { get; set; } = 0.02f; // Default ~50Hz

        public Quaternion attitude
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _attitude != null ? _attitude.attitude.ReadValue() : Quaternion.identity;
#else
                return Input.gyro.attitude;
#endif
            }
        }

        public Vector3 gravity
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _gravity != null ? _gravity.gravity.ReadValue() : Vector3.zero;
#else
                return Input.gyro.gravity;
#endif
            }
        }

        public Vector3 rotationRate
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _gyro != null ? _gyro.angularVelocity.ReadValue() : Vector3.zero;
#else
                return Input.gyro.rotationRate;
#endif
            }
        }

        public Vector3 rotationRateUnbiased
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _gyro != null ? _gyro.angularVelocity.ReadValue() : Vector3.zero;
#else
                return Input.gyro.rotationRateUnbiased;
#endif
            }
        }

        public Vector3 userAcceleration
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _accel != null ? _accel.acceleration.ReadValue() : Vector3.zero;
#else
                return Input.gyro.userAcceleration;
#endif
            }
        }
    }
}
