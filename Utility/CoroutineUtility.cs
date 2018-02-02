using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace BGC.Utility
{
    public static class CoroutineUtility
    {
        private static MonoBehaviour mono = null;
        private static MonoBehaviour Mono
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
            Mono.StartCoroutine(runTimedCoroutine(timeInSeconds, callback));
        }

        private static IEnumerator runTimedCoroutine(float timeInSeconds, Action callback)
        {
            float endTime = Time.time + timeInSeconds;
            do
            {
                yield return null;
            }
            while (Time.time < endTime);

            callback();
        }
    }
}