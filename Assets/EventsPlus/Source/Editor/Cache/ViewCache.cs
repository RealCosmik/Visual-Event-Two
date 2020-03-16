using System.Collections.Generic;
using EventsPlus;
using UnityEditor;
using UnityEngine;
/// <summary>
/// class that handles the delegate cache during editor caches
/// </summary>
public static class ViewCache
{
    /* The DelegateView is the root of all objects in seralized dele
     * //pub[0] //._calls
                //raw[0] //.m_arguments
                        //ref[0]
                        //ref[0]
                        //ref[0]
                //raw[1]
                        //ref[0]
                        //ref[0]
                        //ref[0]
                //raw[2]
                        //ref[0]
                        //ref[0]
                        //ref[0]
     */
    private static Dictionary<int, Dictionary<string, List<VisiualDelegateCacheContainer>>> ObjectCache =
        new Dictionary<int, Dictionary<string, List<VisiualDelegateCacheContainer>>>();
    /// <summary>
    /// Get Dictonary containing property paths and coresponsing publishers on a given object
    /// </summary>
    /// <param name="objectInstanceID">Instance ID of component with publishers</param>
    /// <returns></returns>
    public static Dictionary<string, List<VisiualDelegateCacheContainer>> GetViewDelegateCacheFromObject(int objectInstanceID)
    {
        if (!ObjectCache.TryGetValue(objectInstanceID, out Dictionary<string, List<VisiualDelegateCacheContainer>> PublisherCache))
        {
            PublisherCache = new Dictionary<string, List<VisiualDelegateCacheContainer>>();
            ObjectCache.Add(objectInstanceID, PublisherCache);
        }
        return PublisherCache;
    }
    /// <summary>
    /// Get list of all publisher caches on this given seralizedproperty
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public static List<VisiualDelegateCacheContainer> GetCacheFromPublisherInstance(SerializedProperty property)
    {
        // in the instance that the seralized property is just a field of type publisher than this list will always have one index.
        //if the property is an array than we should populate cache for each element in the array
        var ID = property.serializedObject.targetObject.GetInstanceID();
        var publisherCache = GetViewDelegateCacheFromObject(ID);
        string key = property.GetViewDelegateKey();
        // this instance ID has not been added to the dictonary so its a new object
        if (!publisherCache.TryGetValue(key, out List<VisiualDelegateCacheContainer> instanceCache))
        {
            Debug.LogWarning("<color=green>Creating publisher Cache</color>");
            instanceCache = new List<VisiualDelegateCacheContainer>() { new VisiualDelegateCacheContainer() };
            publisherCache.Add(key, instanceCache);
        }
        else // we have seen this component or GO before
        {
            int index = property.GetViewDelegateIndex(); 
            if (index >= instanceCache.Count) // this is a new publisher so incrase cache size ++;
                instanceCache.Add(new VisiualDelegateCacheContainer());
        }
        return instanceCache;
    }
    /// <summary>
    /// Get PublisherCache Instance from seralized Property
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public static VisiualDelegateCacheContainer GetViewDelegateInstanceCache(SerializedProperty property)
    {
        var instanceCache = GetCacheFromPublisherInstance(property);
        var index = property.GetViewDelegateIndex();
        return instanceCache[index];
    }
    /// <summary>
    /// Returns Generic DelegateCache View from seralized Property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="delegateprop"></param>
    /// <param name="DelegateView"></param>
    /// <returns></returns>
    public static bool GetDelegateView<T>(SerializedProperty delegateprop, out T DelegateView) where T : RawDelegateView
    {
        GetViewDelegateCacheFromObject(delegateprop.serializedObject.targetObject.GetInstanceID());
        if (typeof(T) == typeof(RawCallView))
            DelegateView = GetRawCallCache(delegateprop) as T;
        else if (typeof(T) == typeof(RawDynamicDelegateView))
            DelegateView = GetRawDyanmicDelegateCache(delegateprop) as T;
        else DelegateView = GetRawReferenceCache(delegateprop) as T;
        return DelegateView != null;

    }
    public static RawReferenceView GetRawReferenceCache(SerializedProperty delegateprop)
    {

        var publisherCache = GetViewDelegateInstanceCache(delegateprop);
        int call_index = delegateprop.GetRawCallIndexFromArgumentReference();
        var rawcall_cache = publisherCache.RawCallCache[call_index];
        var argument_list = rawcall_cache.argumentViewCache;
        int Argument_index = delegateprop.GetRawArgumentIndexFromArgumentReference(); 
        return argument_list[Argument_index].argumentReference;
    }
    public static RawArgumentView GetRawArgumentCache(SerializedProperty argumentprop)
    {
        var publisercache = GetViewDelegateInstanceCache(argumentprop);
        int call_index = argumentprop.GetRawCallIndexFromArgumentprop();
        var rawcall_cache = publisercache.RawCallCache[call_index];
        var argument_index = argumentprop.GetRawArgumentIndex();
        if (argument_index >= rawcall_cache.argumentViewCache.Count)
        {
            if (rawcall_cache.delegateView is RawCallView call_view&&call_view.arguments!=null)
            {
                var argtype = call_view.arguments[argument_index].type;
                rawcall_cache.argumentViewCache.Add(new RawArgumentView(argtype));
            }
            else return null;
        }
        return rawcall_cache.argumentViewCache[argument_index];
    }
    public static RawCallView GetRawCallCacheFromRawReference(SerializedProperty property)
    {
        var publisherCache = GetViewDelegateInstanceCache(property);
        int index = property.GetRawCallIndexFromArgumentReference();
        return publisherCache.RawCallCache[index].delegateView as RawCallView;
    }
    public static RawCallView GetRawCallCache(SerializedProperty rawCallProp)
    {
        var publisherCache = GetViewDelegateInstanceCache(rawCallProp);
        int index = rawCallProp.GetRawCallIndex();
        if (index >= publisherCache.RawCallCache.Count) // if the cache list is too small make room for the new cache
        {
            var currentCache = new RawCallViewCacheContainer();
            currentCache.delegateView = new RawCallView(rawCallProp.GetViewDelegateObject()?.types ?? null);
            publisherCache.RawCallCache.Add(currentCache);
        }
        if (publisherCache.RawCallCache[index].dynamic_Cache==true) // if there is a dynamic delegate in a raw call slot remove it
        {
            publisherCache.RawCallCache.RemoveAt(index);
            return null;
        }
        return publisherCache.RawCallCache[index].delegateView as RawCallView;
    }
    public static RawDynamicDelegateView GetRawDyanmicDelegateCache(SerializedProperty DynamicDelegateprop)
    {
        var publisherCache = GetViewDelegateInstanceCache(DynamicDelegateprop);
        int index = DynamicDelegateprop.GetRawCallIndex();
        if (index >= publisherCache.RawCallCache.Count) // if the list is too small make room for the new cache
        { 
            var currentCache = new RawCallViewCacheContainer();
            currentCache.dynamic_Cache = true; // flag that this cache is dynamic
            currentCache.delegateView = new RawDynamicDelegateView();
            publisherCache.RawCallCache.Add(currentCache);
        }
        return publisherCache.RawCallCache[index].delegateView as RawDynamicDelegateView;
    }
}

/// <summary>
/// Cache data for visual Delegates
/// </summary>
public class VisiualDelegateCacheContainer
{
    // on the game side a visualdelegate can have list of raw calls so the visual delegate cache
    //will have a list of rawcalls caches
    public List<RawCallViewCacheContainer> RawCallCache = new List<RawCallViewCacheContainer>();
}
/// <summary>
/// Cache data for rawcalls that reside inside <see cref="VisiualDelegateCacheContainer"/>
/// </summary>
public class RawCallViewCacheContainer
{
    /// <summary>
    /// Delegate view in this cache container
    /// </summary>
    public RawDelegateView delegateView;
    /// <summary>
    /// list of arguments if <see cref="delegateView"/> is a <see cref="RawCallView"/>
    /// </summary>
    public List<RawArgumentView> argumentViewCache = new List<RawArgumentView>();
    /// <summary>
    /// Determining if this cache container is a cache for a <see cref="RawDynamicDelegateView"/>
    /// </summary>
    public bool dynamic_Cache;
}
