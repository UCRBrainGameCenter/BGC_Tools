using System.Threading.Tasks;
using UnityEngine;

namespace BGC.MonoUtility
{
    public abstract class AsyncInitTask : MonoBehaviour
    {
        protected abstract bool PrepareRun();

        protected abstract void FinishedRunning(bool runSuccessful);

        protected abstract Task<bool> ExecuteTask();

        private async void Start()
        {
            if (PrepareRun())
            {
                FinishedRunning(await ExecuteTask());
            }
        }
    }
}
