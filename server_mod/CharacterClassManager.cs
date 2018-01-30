using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEngine.Internal;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngineInternal;

namespace UnityEngine
{
	/// <summary>
	///   <para>Base class for all entities in Unity scenes.</para>
	/// </summary>
	// Token: 0x0200003C RID: 60
	public sealed class GameObject : Object
	{
		/// <summary>
		///   <para>Creates a new game object, named name.</para>
		/// </summary>
		/// <param name="name">The name that the GameObject is created with.</param>
		/// <param name="components">A list of Components to add to the GameObject on creation.</param>
		// Token: 0x06000463 RID: 1123 RVA: 0x0000754F File Offset: 0x0000574F
		public GameObject(string name)
		{
			GameObject.Internal_CreateGameObject(this, name);
		}

		/// <summary>
		///   <para>Creates a new game object, named name.</para>
		/// </summary>
		/// <param name="name">The name that the GameObject is created with.</param>
		/// <param name="components">A list of Components to add to the GameObject on creation.</param>
		// Token: 0x06000464 RID: 1124 RVA: 0x0000755F File Offset: 0x0000575F
		public GameObject()
		{
			GameObject.Internal_CreateGameObject(this, null);
		}

		/// <summary>
		///   <para>Creates a new game object, named name.</para>
		/// </summary>
		/// <param name="name">The name that the GameObject is created with.</param>
		/// <param name="components">A list of Components to add to the GameObject on creation.</param>
		// Token: 0x06000465 RID: 1125 RVA: 0x00007570 File Offset: 0x00005770
		public GameObject(string name, params Type[] components)
		{
			GameObject.Internal_CreateGameObject(this, name);
			foreach (Type componentType in components)
			{
				this.AddComponent(componentType);
			}
		}

		/// <summary>
		///   <para>Creates a game object with a primitive mesh renderer and appropriate collider.</para>
		/// </summary>
		/// <param name="type">The type of primitive object to create.</param>
		// Token: 0x06000466 RID: 1126
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject CreatePrimitive(PrimitiveType type);

		/// <summary>
		///   <para>Returns the component of Type type if the game object has one attached, null if it doesn't.</para>
		/// </summary>
		/// <param name="type">The type of Component to retrieve.</param>
		// Token: 0x06000467 RID: 1127
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern Component GetComponent(Type type);

		// Token: 0x06000468 RID: 1128
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public void GetComponentFastPath(Type type, IntPtr oneFurtherThanResultValue);

		// Token: 0x06000469 RID: 1129 RVA: 0x000075B0 File Offset: 0x000057B0
		[SecuritySafeCritical]
		public unsafe T GetComponent<T>()
		{
			CastHelper<T> castHelper = default(CastHelper<T>);
			this.GetComponentFastPath(typeof(T), new IntPtr((void*)(&castHelper.onePointerFurtherThanT)));
			return castHelper.t;
		}

		// Token: 0x0600046A RID: 1130
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public Component GetComponentByName(string type);

		/// <summary>
		///   <para>Returns the component with name type if the game object has one attached, null if it doesn't.</para>
		/// </summary>
		/// <param name="type">The type of Component to retrieve.</param>
		// Token: 0x0600046B RID: 1131 RVA: 0x000075F0 File Offset: 0x000057F0
		public Component GetComponent(string type)
		{
			return this.GetComponentByName(type);
		}

		/// <summary>
		///   <para>Returns the component of Type type in the GameObject or any of its children using depth first search.</para>
		/// </summary>
		/// <param name="type">The type of Component to retrieve.</param>
		/// <param name="includeInactive"></param>
		/// <returns>
		///   <para>A component of the matching type, if found.</para>
		/// </returns>
		// Token: 0x0600046C RID: 1132
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern Component GetComponentInChildren(Type type, bool includeInactive);

		/// <summary>
		///   <para>Returns the component of Type type in the GameObject or any of its children using depth first search.</para>
		/// </summary>
		/// <param name="type">The type of Component to retrieve.</param>
		/// <param name="includeInactive"></param>
		/// <returns>
		///   <para>A component of the matching type, if found.</para>
		/// </returns>
		// Token: 0x0600046D RID: 1133 RVA: 0x0000760C File Offset: 0x0000580C
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		public Component GetComponentInChildren(Type type)
		{
			return this.GetComponentInChildren(type, false);
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x0000762C File Offset: 0x0000582C
		[ExcludeFromDocs]
		public T GetComponentInChildren<T>()
		{
			bool includeInactive = false;
			return this.GetComponentInChildren<T>(includeInactive);
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x0000764C File Offset: 0x0000584C
		public T GetComponentInChildren<T>([DefaultValue("false")] bool includeInactive)
		{
			return (T)((object)this.GetComponentInChildren(typeof(T), includeInactive));
		}

		/// <summary>
		///   <para>Returns the component of Type type in the GameObject or any of its parents.</para>
		/// </summary>
		/// <param name="type">Type of component to find.</param>
		// Token: 0x06000470 RID: 1136
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern Component GetComponentInParent(Type type);

		// Token: 0x06000471 RID: 1137 RVA: 0x00007678 File Offset: 0x00005878
		public T GetComponentInParent<T>()
		{
			return (T)((object)this.GetComponentInParent(typeof(T)));
		}

		/// <summary>
		///   <para>Returns all components of Type type in the GameObject.</para>
		/// </summary>
		/// <param name="type">The type of Component to retrieve.</param>
		// Token: 0x06000472 RID: 1138 RVA: 0x000076A4 File Offset: 0x000058A4
		public Component[] GetComponents(Type type)
		{
			return (Component[])this.GetComponentsInternal(type, false, false, true, false, null);
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x000076CC File Offset: 0x000058CC
		public T[] GetComponents<T>()
		{
			return (T[])this.GetComponentsInternal(typeof(T), true, false, true, false, null);
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x000076FB File Offset: 0x000058FB
		public void GetComponents(Type type, List<Component> results)
		{
			this.GetComponentsInternal(type, false, false, true, false, results);
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x0000770B File Offset: 0x0000590B
		public void GetComponents<T>(List<T> results)
		{
			this.GetComponentsInternal(typeof(T), false, false, true, false, results);
		}

		/// <summary>
		///   <para>Returns all components of Type type in the GameObject or any of its children.</para>
		/// </summary>
		/// <param name="type">The type of Component to retrieve.</param>
		/// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
		// Token: 0x06000476 RID: 1142 RVA: 0x00007724 File Offset: 0x00005924
		[ExcludeFromDocs]
		public Component[] GetComponentsInChildren(Type type)
		{
			bool includeInactive = false;
			return this.GetComponentsInChildren(type, includeInactive);
		}

		/// <summary>
		///   <para>Returns all components of Type type in the GameObject or any of its children.</para>
		/// </summary>
		/// <param name="type">The type of Component to retrieve.</param>
		/// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
		// Token: 0x06000477 RID: 1143 RVA: 0x00007744 File Offset: 0x00005944
		public Component[] GetComponentsInChildren(Type type, [DefaultValue("false")] bool includeInactive)
		{
			return (Component[])this.GetComponentsInternal(type, false, true, includeInactive, false, null);
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x0000776C File Offset: 0x0000596C
		public T[] GetComponentsInChildren<T>(bool includeInactive)
		{
			return (T[])this.GetComponentsInternal(typeof(T), true, true, includeInactive, false, null);
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x0000779B File Offset: 0x0000599B
		public void GetComponentsInChildren<T>(bool includeInactive, List<T> results)
		{
			this.GetComponentsInternal(typeof(T), true, true, includeInactive, false, results);
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x000077B4 File Offset: 0x000059B4
		public T[] GetComponentsInChildren<T>()
		{
			return this.GetComponentsInChildren<T>(false);
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x000077D0 File Offset: 0x000059D0
		public void GetComponentsInChildren<T>(List<T> results)
		{
			this.GetComponentsInChildren<T>(false, results);
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x000077DC File Offset: 0x000059DC
		[ExcludeFromDocs]
		public Component[] GetComponentsInParent(Type type)
		{
			bool includeInactive = false;
			return this.GetComponentsInParent(type, includeInactive);
		}

		/// <summary>
		///   <para>Returns all components of Type type in the GameObject or any of its parents.</para>
		/// </summary>
		/// <param name="type">The type of Component to retrieve.</param>
		/// <param name="includeInactive">Should inactive Components be included in the found set?</param>
		// Token: 0x0600047D RID: 1149 RVA: 0x000077FC File Offset: 0x000059FC
		public Component[] GetComponentsInParent(Type type, [DefaultValue("false")] bool includeInactive)
		{
			return (Component[])this.GetComponentsInternal(type, false, true, includeInactive, true, null);
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x00007822 File Offset: 0x00005A22
		public void GetComponentsInParent<T>(bool includeInactive, List<T> results)
		{
			this.GetComponentsInternal(typeof(T), true, true, includeInactive, true, results);
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x0000783C File Offset: 0x00005A3C
		public T[] GetComponentsInParent<T>(bool includeInactive)
		{
			return (T[])this.GetComponentsInternal(typeof(T), true, true, includeInactive, true, null);
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x0000786C File Offset: 0x00005A6C
		public T[] GetComponentsInParent<T>()
		{
			return this.GetComponentsInParent<T>(false);
		}

		// Token: 0x06000481 RID: 1153
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public Array GetComponentsInternal(Type type, bool useSearchTypeAsArrayReturnType, bool recursive, bool includeInactive, bool reverse, object resultList);

		// Token: 0x06000482 RID: 1154
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public Component AddComponentInternal(string className);

		/// <summary>
		///   <para>The Transform attached to this GameObject.</para>
		/// </summary>
		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x06000483 RID: 1155
		public extern Transform transform { [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] get; }

		/// <summary>
		///   <para>The layer the game object is in. A layer is in the range [0...31].</para>
		/// </summary>
		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000484 RID: 1156
		// (set) Token: 0x06000485 RID: 1157
		public extern int layer { [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] get; [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000486 RID: 1158
		// (set) Token: 0x06000487 RID: 1159
		[Obsolete("GameObject.active is obsolete. Use GameObject.SetActive(), GameObject.activeSelf or GameObject.activeInHierarchy.")]
		public extern bool active { [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] get; [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] set; }

		/// <summary>
		///   <para>Activates/Deactivates the GameObject.</para>
		/// </summary>
		/// <param name="value">Activate or deactivation the  object.</param>
		// Token: 0x06000488 RID: 1160
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void SetActive(bool value);

		/// <summary>
		///   <para>The local active state of this GameObject. (Read Only)</para>
		/// </summary>
		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000489 RID: 1161
		public extern bool activeSelf { [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] get; }

		/// <summary>
		///   <para>Is the GameObject active in the scene?</para>
		/// </summary>
		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x0600048A RID: 1162
		public extern bool activeInHierarchy { [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] get; }

		// Token: 0x0600048B RID: 1163
		[Obsolete("gameObject.SetActiveRecursively() is obsolete. Use GameObject.SetActive(), which is now inherited by children.")]
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void SetActiveRecursively(bool state);

		/// <summary>
		///   <para>Editor only API that specifies if a game object is static.</para>
		/// </summary>
		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x0600048C RID: 1164
		// (set) Token: 0x0600048D RID: 1165
		public extern bool isStatic { [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] get; [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] set; }

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x0600048E RID: 1166
		public extern bool isStaticBatchable { [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] get; }

		/// <summary>
		///   <para>The tag of this game object.</para>
		/// </summary>
		// Token: 0x170000EB RID: 235
		// (get) Token: 0x0600048F RID: 1167
		// (set) Token: 0x06000490 RID: 1168
		public extern string tag { [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] get; [GeneratedByOldBindingsGenerator] [MethodImpl(MethodImplOptions.InternalCall)] set; }

		/// <summary>
		///   <para>Is this game object tagged with tag ?</para>
		/// </summary>
		/// <param name="tag">The tag to compare.</param>
		// Token: 0x06000491 RID: 1169
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern bool CompareTag(string tag);

		// Token: 0x06000492 RID: 1170
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject FindGameObjectWithTag(string tag);

		/// <summary>
		///   <para>Returns one active GameObject tagged tag. Returns null if no GameObject was found.</para>
		/// </summary>
		/// <param name="tag">The tag to search for.</param>
		// Token: 0x06000493 RID: 1171 RVA: 0x00007888 File Offset: 0x00005A88
		public static GameObject FindWithTag(string tag)
		{
			return GameObject.FindGameObjectWithTag(tag);
		}

		/// <summary>
		///   <para>Returns a list of active GameObjects tagged tag. Returns empty array if no GameObject was found.</para>
		/// </summary>
		/// <param name="tag">The name of the tag to search GameObjects for.</param>
		// Token: 0x06000494 RID: 1172
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject[] FindGameObjectsWithTag(string tag);

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object and on every ancestor of the behaviour.</para>
		/// </summary>
		/// <param name="methodName">The name of the method to call.</param>
		/// <param name="value">An optional parameter value to pass to the called method.</param>
		/// <param name="options">Should an error be raised if the method doesn't exist on the target object?</param>
		// Token: 0x06000495 RID: 1173
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void SendMessageUpwards(string methodName, [DefaultValue("null")] object value, [DefaultValue("SendMessageOptions.RequireReceiver")] SendMessageOptions options);

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object and on every ancestor of the behaviour.</para>
		/// </summary>
		/// <param name="methodName">The name of the method to call.</param>
		/// <param name="value">An optional parameter value to pass to the called method.</param>
		/// <param name="options">Should an error be raised if the method doesn't exist on the target object?</param>
		// Token: 0x06000496 RID: 1174 RVA: 0x000078A4 File Offset: 0x00005AA4
		[ExcludeFromDocs]
		public void SendMessageUpwards(string methodName, object value)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			this.SendMessageUpwards(methodName, value, options);
		}

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object and on every ancestor of the behaviour.</para>
		/// </summary>
		/// <param name="methodName">The name of the method to call.</param>
		/// <param name="value">An optional parameter value to pass to the called method.</param>
		/// <param name="options">Should an error be raised if the method doesn't exist on the target object?</param>
		// Token: 0x06000497 RID: 1175 RVA: 0x000078C0 File Offset: 0x00005AC0
		[ExcludeFromDocs]
		public void SendMessageUpwards(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object value = null;
			this.SendMessageUpwards(methodName, value, options);
		}

		/// <summary>
		///   <para></para>
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="options"></param>
		// Token: 0x06000498 RID: 1176 RVA: 0x000078DB File Offset: 0x00005ADB
		public void SendMessageUpwards(string methodName, SendMessageOptions options)
		{
			this.SendMessageUpwards(methodName, null, options);
		}

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object.</para>
		/// </summary>
		/// <param name="methodName">The name of the method to call.</param>
		/// <param name="value">An optional parameter value to pass to the called method.</param>
		/// <param name="options">Should an error be raised if the method doesn't exist on the target object?</param>
		// Token: 0x06000499 RID: 1177
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void SendMessage(string methodName, [DefaultValue("null")] object value, [DefaultValue("SendMessageOptions.RequireReceiver")] SendMessageOptions options);

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object.</para>
		/// </summary>
		/// <param name="methodName">The name of the method to call.</param>
		/// <param name="value">An optional parameter value to pass to the called method.</param>
		/// <param name="options">Should an error be raised if the method doesn't exist on the target object?</param>
		// Token: 0x0600049A RID: 1178 RVA: 0x000078E8 File Offset: 0x00005AE8
		[ExcludeFromDocs]
		public void SendMessage(string methodName, object value)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			this.SendMessage(methodName, value, options);
		}

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object.</para>
		/// </summary>
		/// <param name="methodName">The name of the method to call.</param>
		/// <param name="value">An optional parameter value to pass to the called method.</param>
		/// <param name="options">Should an error be raised if the method doesn't exist on the target object?</param>
		// Token: 0x0600049B RID: 1179 RVA: 0x00007904 File Offset: 0x00005B04
		[ExcludeFromDocs]
		public void SendMessage(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object value = null;
			this.SendMessage(methodName, value, options);
		}

		/// <summary>
		///   <para></para>
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="options"></param>
		// Token: 0x0600049C RID: 1180 RVA: 0x0000791F File Offset: 0x00005B1F
		public void SendMessage(string methodName, SendMessageOptions options)
		{
			this.SendMessage(methodName, null, options);
		}

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object or any of its children.</para>
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameter"></param>
		/// <param name="options"></param>
		// Token: 0x0600049D RID: 1181
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void BroadcastMessage(string methodName, [DefaultValue("null")] object parameter, [DefaultValue("SendMessageOptions.RequireReceiver")] SendMessageOptions options);

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object or any of its children.</para>
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameter"></param>
		/// <param name="options"></param>
		// Token: 0x0600049E RID: 1182 RVA: 0x0000792C File Offset: 0x00005B2C
		[ExcludeFromDocs]
		public void BroadcastMessage(string methodName, object parameter)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			this.BroadcastMessage(methodName, parameter, options);
		}

		/// <summary>
		///   <para>Calls the method named methodName on every MonoBehaviour in this game object or any of its children.</para>
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameter"></param>
		/// <param name="options"></param>
		// Token: 0x0600049F RID: 1183 RVA: 0x00007948 File Offset: 0x00005B48
		[ExcludeFromDocs]
		public void BroadcastMessage(string methodName)
		{
			SendMessageOptions options = SendMessageOptions.RequireReceiver;
			object parameter = null;
			this.BroadcastMessage(methodName, parameter, options);
		}

		/// <summary>
		///   <para></para>
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="options"></param>
		// Token: 0x060004A0 RID: 1184 RVA: 0x00007963 File Offset: 0x00005B63
		public void BroadcastMessage(string methodName, SendMessageOptions options)
		{
			this.BroadcastMessage(methodName, null, options);
		}

		// Token: 0x060004A1 RID: 1185
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public Component Internal_AddComponentWithType(Type componentType);

		/// <summary>
		///   <para>Adds a component class of type componentType to the game object. C# Users can use a generic version.</para>
		/// </summary>
		/// <param name="componentType"></param>
		// Token: 0x060004A2 RID: 1186 RVA: 0x00007970 File Offset: 0x00005B70
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		public Component AddComponent(Type componentType)
		{
			return this.Internal_AddComponentWithType(componentType);
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x0000798C File Offset: 0x00005B8C
		public T AddComponent<T>() where T : Component
		{
			return this.AddComponent(typeof(T)) as T;
		}

		// Token: 0x060004A4 RID: 1188
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		static extern public void Internal_CreateGameObject([Writable] GameObject mono, string name);

		/// <summary>
		///   <para>Finds a GameObject by name and returns it.</para>
		/// </summary>
		/// <param name="name"></param>
		// Token: 0x060004A5 RID: 1189
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject Find(string name);

		/// <summary>
		///   <para>Scene that the GameObject is part of.</para>
		/// </summary>
		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060004A6 RID: 1190 RVA: 0x000079BC File Offset: 0x00005BBC
		public Scene scene
		{
			get
			{
				Scene result;
				this.INTERNAL_get_scene(out result);
				return result;
			}
		}

		// Token: 0x060004A7 RID: 1191
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		extern public void INTERNAL_get_scene(out Scene value);

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060004A8 RID: 1192 RVA: 0x000079DC File Offset: 0x00005BDC
		public GameObject gameObject
		{
			get
			{
				return this;
			}
		}
	}
}
