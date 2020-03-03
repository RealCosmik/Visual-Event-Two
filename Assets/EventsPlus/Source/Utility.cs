using System;
using System.Text;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
namespace EventsPlus
{
    /// <summary>Utility class for delegate serialization</summary>
    public static class Utility
    {
        /// <summary>
        /// this string is used in replacement of types unityengine.object so that there is no discrepency between .net obj and unity obj
        /// </summary>
        const string UnityobjectSeralizedName = "UnityObject";
        const BindingFlags memberBinding = BindingFlags.Instance|BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        //=======================
        // Serialization
        //=======================
        /// <summary>Serializes a <see cref="System.Reflection.MemberInfo"/> into a string</summary>
        /// <param name="tInfo">MemberInfo to serialize</param>
        /// <returns>String value of the serialized member info, null if invalid</returns>
        public static string Serialize(MemberInfo tInfo)
        {
            List<string> Memberinfo_Data = new List<string>(2);
            if (tInfo != null)
            {
                if (tInfo.MemberType == MemberTypes.Method)
                {
                    StringBuilder tempBuilder = new StringBuilder(((int)tInfo.MemberType).ToString());
                    tempBuilder.Append(":");
                    tempBuilder.Append(tInfo.Name);

                    ParameterInfo[] tempParameters = (tInfo as MethodInfo).GetParameters();
                    int tempListLength = tempParameters.Length;
                    for (int i = 0; i < tempListLength; ++i)
                    {
                        tempBuilder.Append(',');
                        var currentparamType = tempParameters[i].ParameterType;
                        if (currentparamType == typeof(UnityEngine.Object))
                            tempBuilder.Append(UnityobjectSeralizedName);
                        else tempBuilder.Append(tempParameters[i].ParameterType.FullName);
                    }
                    return tempBuilder.ToString();
                }

                return (int)tInfo.MemberType + ":" + tInfo.Name;
            }

            return null;
        }
        public static string[] QuickSeralizer(MemberInfo member_info)
        {

            List<string> member_data = new List<string>(2);
            //first element is member type
            member_data.Add(((int)member_info.MemberType).ToString());
            //second element is member name
            member_data.Add(member_info.Name);
            // if its a method all subsequent elements are the type names of the method parameters
            if (member_info.MemberType == MemberTypes.Method)
            {
                var method_params = (member_info as MethodInfo).GetParameters();
                for (int i = 0; i < method_params.Length; i++)
                    member_data.Add(method_params[i].ParameterType.AssemblyQualifiedName);
            }
            return member_data.ToArray();
        }
        /// <summary>
        /// deseralizes a string array of method data <see cref="QuickSeralizer(MemberInfo)"/> to see seralization format
        /// </summary>
        /// <param name="CurrentType"></param>
        /// <param name="methodata"></param>
        /// <param name="memberflags"></param>
        /// <returns></returns>
        public static MemberInfo QuickDeseralizer(Type CurrentType,string[] methodata,BindingFlags memberflags=memberBinding)
        {
            MemberInfo member_info = null;
            var member_type = (MemberTypes)int.Parse(methodata[0]);
            var member_name = methodata[1];
            if (member_type != MemberTypes.Method) //field or property
            {
                var reflected_members = CurrentType.GetMember(member_name, member_type, memberflags);
                if (reflected_members != null && reflected_members.Length > 0)
                    member_info = reflected_members[0];
            }
            // here we do work if the reflected member is a method
            else
            {
                // data only contiains membertype and method name i.e its a void method
                if (methodata.Length == 2)
                    member_info = CurrentType.GetMethod(member_name, memberflags, null, Type.EmptyTypes, null);
                else
                {
                    var paramtypes = new Type[methodata.Length - 2];
                    for (int i = 2; i < methodata.Length; i++)
                    {
                        paramtypes[i-2] = Type.GetType(methodata[i]);
                    }
                    member_info = CurrentType.GetMethod(member_name, memberflags, null, paramtypes, null);
                }
            }
            return member_info;
        }

        /// <summary>Deserializes a coded string into a <see cref="MemberInfo"/></summary>
        /// <param name="tType">Type of target the desired member belongs</param>
        /// <param name="tSerialized">Raw, serialized form of the member that was created by <see cref="Serialize"/></param>
        /// <param name="tFlags">Binding flags used for the MemberInfo lookup, defaults to all instance members</param>
        /// <returns>MemberInfo reference of the desired member, null if not found</returns>
        public static MemberInfo Deserialize(Type tType, string tSerialized, BindingFlags tFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        {
            //note that members are seralized in the format of "{ mebertype as int}:{method name},{paramtype}...,{paramtype_n}
            Debug.Log(tSerialized);
            MemberInfo Meber_info = null;
            if (!String.IsNullOrEmpty(tSerialized))
            {
                int tempNameIndex = tSerialized.IndexOf(':');
                MemberTypes tempMemberType = (MemberTypes)(Int32.Parse(tSerialized.Substring(0, tempNameIndex)));
                switch (tempMemberType)
                {
                    case MemberTypes.Method:
                        string[] tempRawTypes = tSerialized.Split(',');
                        int tempTypesLength = tempRawTypes.Length - 1;
                        if (tempTypesLength > 0)
                        {
                            Type[] tempTypes = new Type[tempTypesLength];
                            for (int i = (tempTypesLength - 1); i >= 0; --i)
                            {
                                var paramtype = tempRawTypes[i + 1];
                                if (paramtype.Equals(UnityobjectSeralizedName))
                                    tempTypes[i] = typeof(UnityEngine.Object);
                               else  tempTypes[i] = Type.GetType(paramtype);
                            }
                             Meber_info = tType.GetMethod(tempRawTypes[0].Substring(tempNameIndex + 1), tFlags, null, tempTypes, null);
                        }
                        //void method
                        else Meber_info = tType.GetMethod(tempRawTypes[0].Substring(tempNameIndex + 1), tFlags, null, Type.EmptyTypes, null);
                        break;
                    default:
                        MemberInfo[] tempMembers = tType.GetMember(tSerialized.Substring(tempNameIndex + 1), tempMemberType, tFlags);
                        if (tempMembers.Length > 0)
                        {
                            Meber_info = tempMembers[0];
                        }
                        break;
                }
            }
            Debug.Log(Meber_info == null);
            return Meber_info;
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
        /// <summary>
        /// Returns the Visual Event Assembly and the current users working assembly
        /// </summary>
        /// <returns></returns>
        public static Assembly[] GetPublisherAssemblies()
        {
            var publisherAssemblies = new Assembly[2];
            //definitly the tools workspace assembly
            publisherAssemblies[0] = typeof(Utility).Assembly;
            //this is the same assembly as [0] but when the tool get exported to a different project it will be the users active assembly
            publisherAssemblies[1] = Assembly.GetExecutingAssembly();
            return publisherAssemblies;
        }
        public static List<Type> GetTypesInAssemblies()
        {
            var assemblies = GetPublisherAssemblies();
            List<Type> alltypes = new List<Type>();
            for (int i = 0; i < assemblies.Length; i++)
            {
                alltypes.AddRange(assemblies[i].GetTypes());
            }
            return alltypes;
        }
        public static Publisher CreatePublisherFromTypeName(string publishertypeName)
        {
            var alltypes = GetTypesInAssemblies();
            var validtype = alltypes.FirstOrDefault(t => t.FullName.Equals(publishertypeName));
            return Activator.CreateInstance(validtype, null) as Publisher;
        }

        //=======================
        // Delegates
        //=======================
        /// <summary>Gets default flags for all instance members both public and private</summary>
        public static BindingFlags InstanceFlags
        {
            get
            {
                return BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            }
        }
    }
}
