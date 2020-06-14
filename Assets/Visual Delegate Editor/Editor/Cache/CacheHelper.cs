using System.Collections.Generic;
using UnityEditor;
namespace VisualDelegates.Editor
{
    static class CacheHelper
    {
        static Dictionary<string, string[]> ParseData = new Dictionary<string, string[]>();

        /// <summary>
        /// Gets Key of the current ViewDelegateproperty
        /// </summary>
        /// <param name="ViewDelegateproperty"></param>
        /// <returns></returns>
        public static string GetViewDelegateKey(this SerializedProperty ViewDelegateproperty)
        {
            //response.Array.Data[0] // if the  property is an array element then the key will be response i.e the name of th array instance
            //pub // if the property is not an array element then the key will just be the name of the field
            string path = ViewDelegateproperty.propertyPath;
            if (!ParseData.TryGetValue(path, out string[] propertyData))
            {
                propertyData = path.Replace("Array.data", string.Empty).Split('.');
                ParseData.Add(path, propertyData);
            }
            return propertyData[0];
        }
        /// <summary>
        /// Get the index of the current seralized ViewDelegate
        /// </summary>
        /// <param name="ViewDelegateproperty"></param>
        /// <returns></returns>
        public static int GetViewDelegateIndex(this SerializedProperty ViewDelegateproperty)
        {
            ///<see cref="GetViewDelegateKey(SerializedProperty)"/>
            string path = ViewDelegateproperty.propertyPath;
            var propertyData = ParseData[path];
            if (propertyData.Length == 1 || propertyData[1][0] != '[')
            { // if length is 1 the publisher is not in an array so its index in cache is always 0
                return 0;
            }
            else return propertyData[1][1] - '0'; // second elemnt is always [{index}] so index 1 -'0' gives publisher index 

        }
        public static int GetRawCallIndex(this SerializedProperty rawcallProperty)
        {
            //viewdelegate._calls[0] OR viewdeelgateArray[0]._calls[0]  are the potential paths types
            string path = rawcallProperty.propertyPath;
            var propertyData = ParseData[path];
            return propertyData[propertyData.Length - 1][1] - '0';
        }
        /// <summary>
        /// Get the index of the raw call that this raw argument property may belong too
        /// </summary>
        /// <param name="rawArgumentprop"></param>
        /// <returns></returns>
        public static int GetRawCallIndexFromArgumentprop(this SerializedProperty rawArgumentprop)
        {
            //0         //1  //2    //3      //4
            //ViewDelegate._calls[0].m_arguments[0]
            string path = rawArgumentprop.propertyPath;
            var propertydata = ParseData[path];
            // length-3 gives us [0] that is adjacent to ._calls and the [1] is the second charachter in that string which is the index number
            return propertydata[propertydata.Length - 3][1] - '0';
        }
        /// <summary>
        /// Returns the index of the <see cref="RawCallView"/>
        /// </summary>
        /// <param name="rawreferenceprop"></param>
        /// <returns></returns>
        public static int GetRawCallIndexFromArgumentReference(this SerializedProperty rawreferenceprop)
        {
            //0         //1  //2    //3      //4    //5
            //ViewDelegate._calls[0].m_arguments[0].m_reference
            string path = rawreferenceprop.propertyPath;
            var propertydata = ParseData[path];
            return propertydata[propertydata.Length - 4][1] - '0';
        }
        /// <summary>
        /// Gets the index of the rawargument that this argument reference belongs to
        /// </summary>
        /// <param name="rawreferenceprop"></param>
        /// <returns></returns>
        public static int GetRawArgumentIndexFromArgumentReference(this SerializedProperty rawreferenceprop)
        {
            //0             //1  //2    //3     //4    //5
            //ViewDelegate._calls[0].m_arguments[0].m_reference 

            string path = rawreferenceprop.propertyPath;
            var propertydata = ParseData[path];
            return propertydata[propertydata.Length - 2][1] - '0'; // 6-2 =4 which is argument index and then we get the second char
        }
        /// <summary>
        /// Gets the rawargument Index from this rawargument property
        /// </summary>
        /// <param name="rawArgumentprop"></param>
        /// <returns></returns>
        public static int GetRawArgumentIndex(this SerializedProperty rawArgumentprop)
        {
            //0             //1  //2    //3     //4 
            //ViewDelegate._calls[0].m_arguments[0]
            string path = rawArgumentprop.propertyPath;
            var propertydata = ParseData[path];
            return propertydata[propertydata.Length - 1][1] - '0'; //final index is last second charachter is index
        }
    }
}
