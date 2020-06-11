using UnityEngine;
namespace VisualDelegates
{
    public class VisualHooks : MonoBehaviour
    {
        public VisualDelegate onAwake, onStart, onEnable, onDisable, onUpdate, onDestory;
        private void Awake()
        {
            onAwake.initialize();
            onAwake.Invoke();

            onStart.initialize();
            onEnable.initialize();
            onDisable.initialize();
            onUpdate.initialize();
            onDestory.initialize();
        }
        private void Start() => onStart.Invoke();
        private void OnEnable() => onEnable.Invoke();
        private void OnDisable() => onDisable.Invoke();
        private void Update() => onUpdate.Invoke();
        private void OnDestroy()
        {
            onDestory.Invoke();
            onDestory.Release();
            onAwake.Release();
            onEnable.Release();
            onDisable.Release();
            onDestory.Release();
            onUpdate.Release();
        }
    }
}
