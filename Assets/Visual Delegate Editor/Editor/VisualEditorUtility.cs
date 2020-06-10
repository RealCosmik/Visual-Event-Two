using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
namespace VisualDelegates.Editor
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Utility class for editor functions and display</summary>
    public static class VisualEditorUtility
    {
        static Dictionary<string, string[]> ParseData = new Dictionary<string, string[]>();
        public static GUIStyle StandardStyle { get; private set; } = new GUIStyle();
        //=======================
        // Settings
        //=======================
        /// <summary>Menu item that will select the <see cref="Settings"/> object from "Assets/EventsPlus/Resources" folder; this will create one if it doesn't exist</summary>
        [MenuItem("Events Plus/Settings", false)]
        public static void OpenSettings()
        {
            string tempPath = "Assets/EventsPlus/Resources/Settings.asset";
            Settings tempSettings = AssetDatabase.LoadMainAssetAtPath(tempPath) as Settings;
            if (tempSettings == null)
            {
                tempSettings = ScriptableObject.CreateInstance<Settings>();
                AssetDatabase.CreateAsset(tempSettings, AssetDatabase.GenerateUniqueAssetPath(tempPath));
                AssetDatabase.SaveAssets();
                UnityEditor.EditorUtility.FocusProjectWindow();
            }

            Selection.activeObject = tempSettings;
        }
        /// <summary>Converts a type into its short-hand keyword</summary>
        /// <param name="tType">Type to convert</param>
        /// <returns>Keyword if successfully read, null if not</returns>
        public static string GetKeyword(this Type tType)
        {
            if (tType == typeof(void))
            {
                return "void";
            }
            else if (tType == typeof(System.Delegate))
            {
                return "delegate";
            }
            else if (tType == typeof(System.Enum))
            {
                return "enum";
            }
            else if (tType.IsSubclassOf(typeof(Delegate)))
            {

                string delegatename = tType.Name;
                if (tType.IsGenericType)
                {
                    delegatename = delegatename.Substring(0, delegatename.Length - 2);
                    string generic_argumentName = tType.GetGenericArguments()[0].GetKeyword();
                    return $"{delegatename}<{generic_argumentName}>";
                }
                return delegatename;

            }
            else
            {
                switch (Type.GetTypeCode(tType))
                {
                    case TypeCode.Boolean:
                        return "bool";
                    case TypeCode.Byte:
                        return "byte";
                    case TypeCode.Char:
                        return "char";
                    case TypeCode.Decimal:
                        return "decimal";
                    case TypeCode.Double:
                        return "double";
                    case TypeCode.Int16:
                        return "short";
                    case TypeCode.Int32:
                        return "int";
                    case TypeCode.Int64:
                        return "long";
                    case TypeCode.Object:
                        if (tType == typeof(object))
                        {
                            return "object";
                        }

                        return tType.Name;
                    case TypeCode.SByte:
                        return "sbyte";
                    case TypeCode.Single:
                        return "float";
                    case TypeCode.String:
                        return "string";
                    case TypeCode.UInt16:
                        return "ushort";
                    case TypeCode.UInt32:
                        return "uint";
                    case TypeCode.UInt64:
                        return "ulong";
                }
            }

            return null;
        }

        //=======================
        // Serialized Property
        //=======================
        /// <summary>Returns the target object instance that owns <paramref name="tProperty"/></summary>
        /// <param name="tProperty">Property owned by the target instance</param>
        /// <returns>Target instance</returns>
        public static T GetTarget<T>(this SerializedProperty property)
        {
            if (property != null)
            {
                object tempObject = property.serializedObject.targetObject;
                string[] tempPaths = property.propertyPath.Replace("Array.data", "").Split('.');
                int tempListLength = tempPaths.Length;
                for (int i = 0; i < tempListLength; ++i)
                {
                    if (tempPaths[i][0] == '[')
                    {
                        int tempIndex = tempPaths[i][1] - '0';
                        if (tempIndex < (tempObject as IList).Count)
                        {
                            tempObject = (tempObject as IList)[tempIndex];
                        }
                        else
                        {
                            return default;
                        }
                    }
                    else
                    {
                        tempObject = tempObject.GetType().GetField(tempPaths[i], Utility.InstanceFlags).GetValue(tempObject);
                    }
                }
                return (T)tempObject;
            }
            return default;
        }
        /// <summary>Returns the <see cref="VisualDelegateBase"/> instance that owns <paramref name="prop"/></summary>
        /// <param name="prop">Property owned by the Publisher instance</param>
        /// <returns>Publisher instance</returns>
        public static VisualDelegateBase GetVisualDelegateObject(this SerializedProperty prop)
        {
            if (prop != null)
            {
                object tempObject = prop.serializedObject.targetObject;

                string[] tempPaths = prop.propertyPath.Replace("Array.data", "").Split('.');
                int tempListLength = tempPaths.Length;
                for (int i = 0; i < tempListLength; ++i)
                {
                    if (tempPaths[i][0] == '[')
                    {
                        int tempIndex = tempPaths[i][1] - '0';
                        if (tempIndex < (tempObject as IList).Count)
                        {
                            tempObject = (tempObject as IList)[tempIndex];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        tempObject = tempObject.GetType().GetField(tempPaths[i], Utility.InstanceFlags).GetValue(tempObject);
                    }

                    if (tempObject is VisualDelegateBase visiualDelegate)
                        return visiualDelegate;
                }
            }
            return null;
        }
        /// <summary>Returns the <see cref="VisualDelegateBase"/> instance that owns <paramref name="prop"/></summary>
        /// <param name="prop">Property owned by the Publisher instance</param>
        /// <returns>Publisher instance</returns>
        public static FieldInfo GetFieldInfo<T>(this SerializedProperty prop)
        {
            if (prop != null)
            {
                object tempObject = prop.serializedObject.targetObject;
                string[] tempPaths = prop.propertyPath.Replace("Array.data", "").Split('.');
                int tempListLength = tempPaths.Length;
                for (int i = 0; i < tempListLength; ++i)
                {
                    if (tempPaths[i][0] == '[')
                    {
                        int tempIndex = tempPaths[i][1] - '0';
                        if (tempIndex < (tempObject as IList).Count)
                        {
                            tempObject = (tempObject as IList)[tempIndex];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        tempObject = tempObject.GetType().GetField(tempPaths[i], Utility.InstanceFlags).GetValue(tempObject);
                    }
                }
            }
            return null;
        }

        //=======================
        // Namespaces
        //=======================
        /// <summary>Returns a list of all assembly <see cref="Namespace"/>s that contain <see cref="UnityEngine.Object"/> classes</summary>
        /// <returns>List of namespaces</returns>
        public static List<Namespace> GetUnityObjectNamespaces()
        {
            // Generate
            List<Type> tempClasses;
            Dictionary<string, List<Type>> tempNamespaces = null;
            Assembly[] tempAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = (tempAssemblies.Length - 1); i >= 0; --i)
            {
                if (tempAssemblies[i].FullName.IndexOf("UnityEditor") < 0)
                {
                    Type[] tempTypes = tempAssemblies[i].GetTypes();
                    Type tempType;
                    for (int j = (tempTypes.Length - 1); j >= 0; --j)
                    {
                        tempType = tempTypes[j];
                        if (tempType == typeof(UnityEngine.Object) || tempType.IsSubclassOf(typeof(UnityEngine.Object)))
                        {
                            bool tempIsNamespace = tempType.Namespace != null;
                            if (!tempIsNamespace || tempType.Namespace.IndexOf("UnityEditor") < 0)
                            {
                                string tempNamespace = tempIsNamespace ? tempType.Namespace : "NONE";
                                if (tempNamespaces == null)
                                {
                                    tempNamespaces = new Dictionary<string, List<Type>>();
                                    tempClasses = new List<Type>();
                                    tempClasses.Add(tempType);
                                    tempNamespaces.Add(tempNamespace, tempClasses);
                                }
                                else if (tempNamespaces.TryGetValue(tempNamespace, out tempClasses))
                                {
                                    tempClasses.Add(tempType);
                                }
                                else
                                {
                                    tempClasses = new List<Type>();
                                    tempClasses.Add(tempType);
                                    tempNamespaces.Add(tempNamespace, tempClasses);
                                }
                            }
                        }
                    }
                }
            }

            // Compile and order
            tempClasses = new List<Type>();
            tempClasses.Add(typeof(object));
            List<Namespace> tempOut = new List<Namespace>();
            tempOut.Add(new Namespace("System", tempClasses));

            if (tempNamespaces != null)
            {
                foreach (KeyValuePair<string, List<Type>> tempPair in tempNamespaces)
                {
                    tempPair.Value.Sort((tA, tB) => string.Compare(tA.Name, tB.Name));
                    tempOut.Add(new Namespace(tempPair.Key, tempPair.Value));
                }

                tempOut.Sort((tA, tB) => string.Compare(tA.name, tB.name));
            }

            return tempOut;
        }

        //=======================
        // Members
        //=======================
        /// <summary>Returns a list of all <see cref="IMember"/>s belonging to the <paramref name="CurrentType"/></summary>
        /// <param name="CurrentType">Type to search</param>
        /// <param name="tIsFiltered">If true, will attempt to filter members defined in the <see cref="Settings"/></param>
        /// <returns>List of members</returns>
        public static List<IMember> GetMemberList(this Type CurrentType, bool tIsFiltered = true, bool staticmembers = false)
        {
            if (CurrentType != null)
            {
                List<IMember> tempMembers = null;

                // Fields
                List<MemberField> tempFields = CurrentType.GetFieldList(tIsFiltered);
                if (tempFields != null)
                {
                    if (tempMembers == null)
                    {
                        tempMembers = new List<IMember>();
                    }

                    int tempListLength = tempFields.Count;
                    for (int i = 0; i < tempListLength; ++i)
                    {
                        tempMembers.Add(tempFields[i]);
                    }
                }

                // Properties
                List<MemberProperty> tempProperties = CurrentType.GetPropertyList(tIsFiltered);
                if (tempProperties != null)
                {
                    if (tempMembers == null)
                    {
                        tempMembers = new List<IMember>();
                    }

                    int tempListLength = tempProperties.Count;
                    for (int i = 0; i < tempListLength; ++i)
                    {
                        tempMembers.Add(tempProperties[i]);
                    }
                }

                // Methods
                List<MemberMethod> tempMethods = CurrentType.GetMethodList(tIsFiltered);
                if (tempMethods != null)
                {
                    if (tempMembers == null)
                    {
                        tempMembers = new List<IMember>();
                    }

                    int tempListLength = tempMethods.Count;
                    for (int i = 0; i < tempListLength; ++i)
                    {
                        tempMembers.Add(tempMethods[i]);
                    }
                }

                return tempMembers;
            }

            return null;
        }

        /// <summary>Returns a list of all <see cref="MemberField"/>s belonging to the <paramref name="tType"/></summary>
        /// <param name="tType">Type to search</param>
        /// <param name="tIsFiltered">If true, will attempt to filter fields defined in the <see cref="Settings"/></param>
        /// <returns>List of fields</returns>
        public static List<MemberField> GetFieldList(this Type tType, bool tIsFiltered = true)
        {
            if (tType != null)
            {
                // Flags
                BindingFlags tempFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
                // Filter member fields
                FieldInfo[] tempFields = tType.GetFields(tempFlags);
                int tempListLength = tempFields.Length;
                if (tempListLength > 0)
                {
                    List<MemberField> tempOut = new List<MemberField>();
                    FieldInfo tempField;
                    for (int i = 0; i < tempListLength; ++i)
                    {
                        tempField = tempFields[i];
                        if (!tempField.IsInitOnly && !tempField.IsLiteral) //not readonly field and not const field
                        {
                            MemberField tempMember = new MemberField(tempField);
                            if (tempField.IsPrivate)
                            {
                                if(tempField.GetCustomAttribute<DisplayPrivate>()!=null)
                                tempOut.Add(tempMember);
                            }
                            else tempOut.Add(tempMember);
                        }
                    }
                    return tempOut;
                }
            }
            return null;
        }

        /// <summary>Returns a list of all <see cref="MemberProperty"/>s belonging to the <paramref name="tType"/></summary>
        /// <param name="tType">Type to search</param>
        /// <param name="tIsFiltered">If true, will attempt to filter properties defined in the <see cref="Settings"/></param>
        /// <returns>List of properties</returns>
        public static List<MemberProperty> GetPropertyList(this Type tType, bool tIsFiltered = true)
        {
            if (tType != null)
            {
                // Flags
                BindingFlags tempFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

                // Filter member properties
                PropertyInfo[] tempProperties = tType.GetProperties(tempFlags);
                int tempListLength = tempProperties.Length;
                if (tempListLength > 0)
                {
                    List<MemberProperty> tempOut = new List<MemberProperty>();
                    PropertyInfo tempProperty;
                    for (int i = 0; i < tempListLength; ++i)
                    {
                        tempProperty = tempProperties[i];
                        if (tempProperty.CanWrite)
                        {
                            MemberProperty tempMember = new MemberProperty(tempProperty);
                            if (tempProperty.SetMethod.IsPrivate)
                            {
                                if (tempProperty.GetCustomAttribute<DisplayPrivate>() != null)
                                    tempOut.Add(tempMember);
                            }
                            else tempOut.Add(tempMember);
                        }
                    }

                    return tempOut;
                }
            }

            return null;
        }

        /// <summary>Returns a list of all <see cref="MemberMethod"/>s belonging to the <paramref name="tType"/></summary>
        /// <param name="tType">Type to search</param>
        /// <param name="tIsFiltered">If true, will attempt to filter methods defined in the <see cref="Settings"/></param>
        /// <returns>List of methods</returns>
        public static List<MemberMethod> GetMethodList(this Type tType, bool tIsFiltered = true)
        {
            if (tType != null)
            {
                // Flags
                BindingFlags tempFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
                // Filter member methods
                MethodInfo[] tempMethods = tType.GetMethods(tempFlags);
                int tempListLength = tempMethods.Length;
                if (tempListLength > 0)
                {
                    List<MemberMethod> tempOut = new List<MemberMethod>();
                    MethodInfo tempMethod;
                    for (int i = 0; i < tempListLength; ++i)
                    {
                        tempMethod = tempMethods[i];
                        if (!tempMethod.IsSpecialName && !tempMethod.IsGenericMethod)
                        {
                            MemberMethod tempMember = new MemberMethod(tempMethod);
                            if (tempMethod.IsPrivate)
                            {
                                if (tempMethod.GetCustomAttribute<DisplayPrivate>() != null)
                                    tempOut.Add(tempMember);
                            }
                            else tempOut.Add(tempMember);
                        }
                    }

                    return tempOut;
                }
            }

            return null;
        }
        /// <summary>
        /// Filters a group of Imembers by Type
        /// </summary>
        /// <param name="members"></param>
        /// <param name="matchtype"></param>
        /// <returns></returns>
        public static List<IMember> GetMemberList(this IEnumerable<IMember> members, Type matchtype)
        {
            return members.Where(m => filterGetterMembersByReturnType(m, matchtype)).ToList();
        }
        /// <summary>
        /// Filters an <see cref="IMember"/> if they match the given return type
        /// </summary>
        /// <param name="member"></param>
        /// <param name="ReturnType"></param>
        /// <returns></returns>
        private static bool filterGetterMembersByReturnType(IMember member, Type ReturnType)
        {
            var istypestring = ReturnType == typeof(string);
            switch (member.info.MemberType)
            {
                case MemberTypes.Field:
                    if (istypestring)
                        return true;
                    else return (member.info as FieldInfo).FieldType == ReturnType;
                case MemberTypes.Property:
                    var prop_info = member.info as PropertyInfo;
                    if (istypestring)
                        return prop_info.CanRead; //if the returntype is a string any property will suffice because we can just .ToString() it
                    else return prop_info.CanRead && prop_info.PropertyType == ReturnType;
                case MemberTypes.Method:
                    var method_info = member.info as MethodInfo;
                    return method_info.ReturnType == ReturnType && method_info.GetParameters().Length == 0;
                default:
                    return false;
            }
        }
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
        /// <summary>
        /// Copies Seralized MemberData Data into a seralziedProperty 
        /// </summary>
        /// <param name="methodDataprop"></param>
        /// <param name="seralizedData"></param>
        public static void CopySeralizedMethodDataToProp(SerializedProperty methodDataprop, string[] seralizedData)
        {
            methodDataprop.arraySize = seralizedData.Length;
            for (int i = 0; i < seralizedData.Length; i++)
            {
                methodDataprop.GetArrayElementAtIndex(i).stringValue = seralizedData[i];
            }
        }
        /// <summary>
        /// Creates an array message for a seralizedmethod data that cannto be found 
        /// </summary>
        /// <param name="methodData_prop"></param>
        /// <param name="ErrorObject"></param>
        /// <returns></returns>
        public static string CreateErrorMessage(SerializedProperty methodData_prop, UnityEngine.Object ErrorObject)
        {
            var member_type = (MemberTypes)Utility.ConvertStringToInt(methodData_prop.GetArrayElementAtIndex(0).stringValue);
            var member_name = methodData_prop.GetArrayElementAtIndex(1).stringValue;
            return $@"{member_type}: ""{member_name}"" was removed or renamed in type: ""{ErrorObject.GetType()}""";
        }
        /// <summary>
        /// Copies the data between 2 <see cref="RawArgument"/> references
        /// </summary>
        /// <param name="DestinationArgument"></param>
        /// <param name="originargument"></param>
        public static void CopyDelegateArguments(SerializedProperty DestinationArgument, SerializedProperty originargument)
        {
            DestinationArgument.FindPropertyRelative("objectValue").objectReferenceValue = originargument.FindPropertyRelative("objectValue").objectReferenceValue;
            DestinationArgument.FindPropertyRelative("_x1").floatValue = originargument.FindPropertyRelative("_x1").floatValue;
            DestinationArgument.FindPropertyRelative("_x2").floatValue = originargument.FindPropertyRelative("_x2").floatValue;
            DestinationArgument.FindPropertyRelative("_y1").floatValue = originargument.FindPropertyRelative("_y1").floatValue;
            DestinationArgument.FindPropertyRelative("_y2").floatValue = originargument.FindPropertyRelative("_y2").floatValue;
            DestinationArgument.FindPropertyRelative("_z1").floatValue = originargument.FindPropertyRelative("_z1").floatValue;
            DestinationArgument.FindPropertyRelative("_z2").floatValue = originargument.FindPropertyRelative("_z2").floatValue;
            DestinationArgument.FindPropertyRelative("stringValue").stringValue = originargument.FindPropertyRelative("stringValue").stringValue;
            DestinationArgument.FindPropertyRelative("longValue").longValue = originargument.FindPropertyRelative("longValue").longValue;
            DestinationArgument.FindPropertyRelative("doubleValue").doubleValue = originargument.FindPropertyRelative("doubleValue").doubleValue;
            DestinationArgument.FindPropertyRelative("animationCurveValue").animationCurveValue = originargument.FindPropertyRelative("animationCurveValue").animationCurveValue;
        }
        public static async void TweenBox(Rect boxpos, VisiualDelegateCacheContainer containter)
        {
            containter.color = DelegateEditorSettings.instance.InvocationColor;
            if (!containter.istweening)
            {
                containter.istweening = true;
                while (containter.color.a > 0f)
                {
                    containter.color.a -= .07f;
                    await Task.Delay(50);
                }
                containter.istweening = false;
            }
            else containter.color.a = 1f;
        }
        public static void ReinitializeDelegate(VisualDelegateBase del)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            if (del is VisualDelegate)
                del.GetType().GetField("m_onInvoke", flags).SetValue(del, null);
            else del.GetType().BaseType.GetField("m_onInvoke", flags).SetValue(del, null);
            del.initialize();
        }
        public static string ParseDynamicMethodName(string methodname)
        {
            if (methodname[0] == '<')//anon method
            {
                return $@"Method: Anonymous method created in ""{methodname.Substring(1, methodname.IndexOf('>') - 1)}""";
            }
            else return $@"Method: ""{methodname}""";
        }
        public static string ParseDynamicTargetName(string TargetType)
        {
            if (TargetType.Contains("+"))
            {
                return $@"Target: Anonymous Type Created in ""{TargetType.Substring(0, TargetType.IndexOf('+'))}""";
            }
            return $@"Target: ""{TargetType}""";
        }
        public static PropertyName STRING_TYPE_NAME = "System.String";
        public static PropertyName CHAR_TYPE_NAME = "System.Char";
        public static PropertyName DOTNET_TYPE_NAME = "System.Type";
        public static PropertyName BOOLEAN_TYPE_NAME = "System.Boolean";
        public static PropertyName INTEGER_TYPE_NAME = "System.Int32";
        public static PropertyName LONG_TYPE_NAME = "System.Int64";
        public static PropertyName FLOAT_TYPE_NAME = "System.Single";
        public static PropertyName DOUBLE_TYPE_NAME = "System.Double";
        public static PropertyName VECTOR2_TYPE_NAME = "UnityEngine.Vector2";
        public static PropertyName VECTOR3_TYPE_NAME = "UnityEngine.Vector3";
        public static PropertyName VECTOR4_TYPE_NAME = "UnityEngine.Vector4";
        public static PropertyName QUATERNION_TYPE_NAME = "UnityEngine.Quaternion";
        public static PropertyName UNITYRECT_TYPE_NAME = "UnityEngine.Rect";
        public static PropertyName UNITYBOUNDS_TYPE_NAME = "UnityEngine.Bounds";
        public static PropertyName UNITYCOLOR_TYPE_NAME = "UnityEngine.Color";
        public static PropertyName UNITYCURVE_TYPE_NAME = "UnityEngine.AnimationCurve";
        public static PropertyName UNITYOBJECt_TYPE_NAME = "UnityEngine.Object";
        //=======================
        // Inspector
        //=======================
        /// <summary>Gets the width of Unity's indents in the inspector</summary>
        public static float IndentSize
        {
            get
            {
                return 15;
            }
        }
    }
}