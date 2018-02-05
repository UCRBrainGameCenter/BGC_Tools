using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace BGC.Utility
{
    public static class CoroutineUtility
    {
        private static MonoBehaviour mono = null;
        public static MonoBehaviour Mono
        {
            get
            {
                if (mono == null)
                {
                    mono = new GameObject().AddComponent<EmptyMonobehaviour>();
                }

                return mono;
            }
        }

        /// <summary>
        /// Run an array of routines with the option to start them with StartCoroutine
        /// and call a callback on completion
        /// </summary>
        /// <param name="routines"></param>
        /// <param name="startRoutines"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IEnumerator RunRoutines(IEnumerator[] routines, bool startRoutines = false, Action callback = null)
        {
            if (startRoutines)
            {
                for (int i = 0; i < routines.Length; ++i)
                {
                    Mono.StartCoroutine(routines[i]);
                }
            }

            bool running = true;
            while (running)
            {
                yield return null;
                bool stop = true;
                for (int i = 0; i < routines.Length; ++i)
                {
                    if (routines[i].MoveNext())
                    {
                        stop = false;
                        break;
                    }
                }

                if (stop)
                {
                    running = false;
                }
            }

            if (callback != null)
            {
                callback();
            }
        }

        /// <summary>
        /// Run a list of routines with the option to start them with StartCoroutine
        /// and call a callback on completion
        /// </summary>
        /// <param name="routines"></param>
        /// <param name="startRoutines"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IEnumerator RunRoutines(List<IEnumerator> routines, bool startRoutines = false, Action callback = null)
        {
            if (startRoutines)
            {
                for (int i = 0; i < routines.Count; ++i)
                {
                    Mono.StartCoroutine(routines[i]);
                }
            }

            bool running = true;
            while (running)
            {
                yield return null;
                bool stop = true;
                for (int i = 0; i < routines.Count; ++i)
                {
                    if (routines[i].MoveNext())
                    {
                        stop = false;
                        break;
                    }
                }

                if (stop)
                {
                    running = false;
                }
            }

            if (callback != null)
            {
                callback();
            }
        }

        /// <summary>
        /// Run function after x time in seconds
        /// </summary>
        /// <param name="timeInSeconds"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void RunFunctionAfterTime(float timeInSeconds, Action callback)
        {
            Mono.StartCoroutine(TimedCoroutine(timeInSeconds, callback));
        }

        /// <summary>
        /// Create a timed coroutine that will call a callback after the given
        /// time in seconds
        /// </summary>
        /// <param name="timeInSeconds"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IEnumerator TimedCoroutine(float timeInSeconds, Action callback)
        {
            float endTime = Time.time + timeInSeconds;
            do
            {
                yield return null;
            }
            while (Time.time < endTime);

            callback();
        }

        /// <summary>
        /// Run for a given period of time in seconds and provide a call back that will
        /// be called for each tick of yield return null. This function will provide the
        /// time backwards (meaning 60, 59, 58, ...)
        /// </summary>
        /// <param name="timeInSeconds"></param>
        /// <param name="callbackOnTick"></param>
        /// <param name="completedCallback"></param>
        /// <returns></returns>
        public static IEnumerator TimedCoroutineBackwardsTime(float timeInSeconds, Action<float> callbackOnTick, Action completedCallback)
        {
            do
            {
                yield return null;
                timeInSeconds -= Time.deltaTime;
                callbackOnTick(timeInSeconds);
            }
            while (timeInSeconds > 0);

            completedCallback();
        }

        /// <summary>
        /// Run for a given period of time in seconds and provide a call back that will
        /// be called for each tick of yield return null. This function will provide the
        /// time forwards (meaning 0, 1, 2, ...)
        /// </summary>
        /// <param name="timeInSeconds"></param>
        /// <param name="callbackOnTick"></param>
        /// <param name="completedCallback"></param>
        /// <returns></returns>
        public static IEnumerator TimedCoroutineForwardTime(float timeInSeconds, Action<float> callbackOnTick, Action completedCallback)
        {
            float time = 0f;

            do
            {
                yield return null;
                time += Time.deltaTime;
                callbackOnTick(time);
            }
            while (time < timeInSeconds);

            completedCallback();
        }
    }
}