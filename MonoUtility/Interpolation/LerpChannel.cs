using System;
using System.Collections;
using UnityEngine;

namespace BGC.MonoUtility.Interpolation
{
    /// <summary>
    /// Monobehavior to execute and manage animation-like actions.
    /// </summary>
    public abstract class LerpChannel<T> : MonoBehaviour
    {
        protected float duration;

        protected abstract T Target { get; }

        protected ILerpAction<T> lerpAction = null;
        protected IContinuousAction<T> continuousAction = null;
        protected Action<T> finishedCallback = null;
        protected Action<T> interruptedCallback = null;

        protected bool running = false;

        public float ElapsedTime { get; private set; }

        private int recurrentInterruptIdentifier = 0;
        private IEnumerator lerpCoroutine = null;

        public void Activate(
            float duration,
            ILerpAction<T> lerpAction = null,
            IContinuousAction<T> continuousAction = null,
            Action<T> finishedCallback = null,
            Action<T> interruptedCallback = null)
        {
            if (running)
            {
                if (this.interruptedCallback != null)
                {
                    //Handle existing interruptedCallbacks
                    if (recurrentInterruptIdentifier > 2)
                    {
                        Debug.LogError("Identified potentially recursive interrupt callbacks");
                    }

                    ++recurrentInterruptIdentifier;

                    Action<T> tempInterruptedCallback = this.interruptedCallback;
                    this.interruptedCallback = interruptedCallback;
                    tempInterruptedCallback.Invoke(Target);

                    --recurrentInterruptIdentifier;
                }
            }

            ElapsedTime = 0f;

            this.duration = duration;

            this.lerpAction = lerpAction;
            this.continuousAction = continuousAction;
            this.finishedCallback = finishedCallback;
            this.interruptedCallback = interruptedCallback;

            this.lerpAction?.Initialize(Target);
            this.continuousAction?.Initialize(Target, Time.time);

            if (!running)
            {
                StartCoroutine(lerpCoroutine = RunAction());
            }
        }

        /// <summary>
        /// Stops the running LerpChannel.
        /// This will likely execute the InterruptedCallback.
        /// </summary>
        public void Kill()
        {
            running = false;
            //The InterruptedCallback action won't be called if Kill is executed on the same frame as
            //the LerpChannel finishes.  This is by design.
        }

        /// <summary>
        /// Stops the running LerpChannel and terminates the Coroutine.
        /// This will not execute the InterruptedCallback.
        /// </summary>
        public void HardKill()
        {
            running = false;

            if (lerpCoroutine?.MoveNext() ?? false)
            {
                StopCoroutine(lerpCoroutine);

                finishedCallback = null;
                lerpAction = null;
                continuousAction = null;
                interruptedCallback = null;
            }
        }

        private IEnumerator RunAction()
        {
            running = true;

            lerpAction?.CallAction(0f);
            continuousAction?.CallAction(Time.time);

            do
            {
                yield return null;

                ElapsedTime += Time.unscaledDeltaTime;

                //As per best-practices, we are not using null-conditional operators in
                //every-frame loops
                if (lerpAction != null)
                {
                    lerpAction.CallAction(ElapsedTime / duration);
                }

                if (continuousAction != null)
                {
                    continuousAction.CallAction(Time.time);
                }
            }
            while (ElapsedTime < duration && running);

            running = false;

            if (ElapsedTime < duration)
            {
                //If ElapsedTime < duration, that means we bailed out when running was set to false
                //and thus didn't execute lerpAction.CallAction(1f)
                if (interruptedCallback != null)
                {
                    //Execute our interrupted callback
                    ++recurrentInterruptIdentifier;

                    //Cache the value because executing it could could change or clear the state.
                    Action<T> tempInterruptedCallback = interruptedCallback;

                    finishedCallback = null;
                    interruptedCallback = null;
                    lerpAction = null;
                    continuousAction = null;

                    tempInterruptedCallback.Invoke(Target);

                    --recurrentInterruptIdentifier;
                }
            }
            else if (finishedCallback != null)
            {
                //Copy the callback in case it triggers a new coroutine
                Action<T> tempFinishedCallback = finishedCallback;

                finishedCallback = null;
                lerpAction = null;
                continuousAction = null;
                interruptedCallback = null;

                tempFinishedCallback.Invoke(Target);
            }

        }
    }

}