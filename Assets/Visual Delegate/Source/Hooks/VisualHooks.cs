using UnityEngine;
namespace VisualDelegates
{
    public class VisualHooks : MonoBehaviour
    {
        public VisualDelegate onAwake, onStart, onEnable, onDisable, onUpdate, onDestory;
        [SerializeField] string onAwakeNote, onStartNote, onEnableNote, onDisableNote, onUpdateNote, onDestoryNote;
        [SerializeField] bool LogHooks;
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
        private void Start()
        {
            onStart.Invoke();
            Hooklog("Start hook executed");
        }
        private void OnEnable()
        {
            onEnable.Invoke();
            Hooklog("Start hook executed");
        }
        private void OnDisable()
        {
            onDisable.Invoke();
            Hooklog("Disalbe Hook executed"); 
        }
        private void Update()
        {
            onUpdate.Invoke();
            Hooklog("Update Hook executed");
        }
        private void OnDestroy()
        {
            onDestory.Invoke();
            Hooklog("Destory Hook executed");
            onDestory.Release();
            onAwake.Release();
            onEnable.Release();
            onDisable.Release();
            onDestory.Release();
            onUpdate.Release();
        }
        private void Hooklog(string message)
        {
            if (Application.isEditor&&LogHooks)
                Debug.Log(message,this);

        }
    }
}
