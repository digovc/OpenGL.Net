﻿
// Copyright (C) 2009-2016 Luca Piccioni
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
// USA

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenGL
{
	/// <summary>
	/// Graphics context.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A GraphicsContext represents an attachment to the graphic operations. It actually store the graphic state, which determines the
	/// graphical pipeline results.
	/// </para>
	/// <para>
	/// To construct a GraphicsContext instance, it is necessary a device context. The device context is a OS handle which represents a
	/// specific graphical device. You can note that there are constructors which does not require a device context parameter: in this cases
	/// a common device context is used implicitly.
	/// </para>
	/// <para>
	/// If it available, it is possible to request a particoular OpenGL implementation. To check availability, <see cref="GraphicsContext.Capabilities.CreateContext"/>
	/// or <see cref="GraphicsContext.Capabilities.CreateContextProfile"/> indicates whether it is possible to select a particoular OpenGL implementation
	/// only for a GraphicsContext instance.
	/// 
	/// Respect a particoular version requested, it is possible that the current OpenGL implementation returns a context that doesn't have the exact
	/// OpenGL version requested. In this cases it is assured that the OpenGL version requested it is implemented (i.e. actual OpenGL version is a
	/// superset of the requested one).
	/// 
	/// Without those OpenGL extensions, it won't be possible to request a different OpenGL implementation from the current one, which is queriable
	/// by accessing to <see cref="GraphicsContext.CurrentCaps"/>.
	/// </para>
	/// <para>
	/// It is possible to share resources with other GraphicsContext instances by specifying a GraphicsContext parameter at construction time. The
	/// resources could be shared are listed in <see cref="IGraphicsResource"/>. There can be sharing compatibility issues by sharing resources
	/// having different OpenGL implementations.
	/// 
	/// As generale rule, when an OpenGL version introduce a new object space class not implemented by another version, those two OpenGL version
	/// cannot share object spaces.
	/// </para>
	/// <para>
	/// GraphicsContext define OpenGL implementation capabilities with the type <see cref="GraphicsContext.Capabilities"/>. This type collection
	/// useful information about a specific OpenGL implementation. It defines general information, implementation limits and extension support.
	/// 
	/// GraphicsContext exposes the current implementation capabilities by the static property <see cref="GraphicsContext.CurrentCaps"/>. This information
	/// is static, and it represent the most extended implementation currently available. Normally this property is not used for testing OpenGL support
	/// and limits, since this information is dependent on the current context, which could not be a GraphicsContext with the current OpenGL version (because
	/// it is possible to request a specific OpenGL implementation version).
	/// 
	/// Since each OpenGL implementation version can support any OpenGL extension combination, each GraphicsContext has its own OpenGL support, which could
	/// differ from the current OpenGL support.
	/// 
	/// Because this, all methods which depends on OpenGL support take a parameter of type GraphicsContext: that parameter is used to test effective support
	/// and limits for the specific OpenGL context. Testing OpenGL context capabilities by means of <see cref="GraphicsContext.CurrentCaps"/> could lead to
	/// invalid operations since those capabilities are not referred to the currently bound context.
	/// </para>
	/// <para>
	/// Since the device context is constructed by specifying a device context, it is able to detect ... remove public interface!
	/// </para>
	/// <para>
	/// Before any operation, the GraphicsContext has to be current. Only one current context per thread is allowed, but multiple context can made current on
	/// a thread one at time. Once a GraphicsContext is current on a thread, it is possible to issue rendering commands, and the rendering commands result is
	/// dependent on the current GraphicsContext.
	/// 
	/// Issuing rendering commands without having a current context on the thread will lead to exceptions.
	/// 
	/// A GraphicsContext is made current by calling <see cref="MakeCurrent(System.Boolean)"/> or <see cref="MakeCurrent(System.IntPtr,System.Boolean)"/>. It is
	/// possible to check the GraphicsContext currency by calling <see cref="IsCurrent()"/>, or getting the current GraphicsContext for the thread
	/// by calling <see cref="GetCurrentContext"/>.
	/// </para>
	/// </remarks>
	public sealed partial class GraphicsContext : IDisposable
	{
		#region Constructors

		#region Constructor Overrides -  Constructors specifying DeviceContext

		/// <summary>
		/// Construct a GraphicsContext.
		/// </summary>
		/// <param name="deviceContext">
		/// A <see cref="DeviceContext"/> that specify the device context which has to be linked this
		/// this Render context.
		/// </param>
		/// <param name="version">
		/// A <see cref="KhronosVersion"/> that specify the minimum OpenGL version required to implement.
		/// </param>
		/// <exception cref="ArgumentException">
		/// This exception is thrown in the case <paramref name="version"/> specify a forward compatible version (greater than or equal to
		/// <see cref="GLVersion.Version_3_2"/>), and the OpenGL extension WGL_ARB_create_context_profile or WGL_ARB_create_context
		/// are not implemented.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown in the case <paramref name="deviceContext"/> is <see cref="IntPtr.Zero"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// This exception is thrown in the case it's not possible to create a valid OpenGL context.
		/// </exception>
		public GraphicsContext(DeviceContext deviceContext) :
			this(deviceContext, null)
		{

		}

		/// <summary>
		/// Construct a GraphicsContext specifying the implemented OpenGL version.
		/// </summary>
		/// <param name="deviceContext">
		/// A <see cref="DeviceContext"/> that specify the device context which has to be linked this
		/// this Render context.
		/// </param>
		/// <param name="sharedContext">
		/// A <see cref="GraphicsContext"/> that specify the render context which has to be linked this
		/// this Render context (to share resource with it).
		/// </param>
		/// <exception cref="ArgumentException">
		/// This exception is thrown in the case <paramref name="deviceContext"/> is <see cref="IntPtr.Zero"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// This exception is thrown in the case it's not possible to create a valid OpenGL context.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown if <paramref name="sharedContext"/> is not null and it was created by a thread different from the calling one.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown if <paramref name="sharedContext"/> is not null and it is disposed.
		/// </exception>
		public GraphicsContext(DeviceContext deviceContext, GraphicsContext sharedContext) :
			this(deviceContext, sharedContext, Gl.CurrentVersion, GraphicsContextFlags.None)
		{

		}

		/// <summary>
		/// Construct a GraphicsContext specifying the implemented OpenGL version.
		/// </summary>
		/// <param name="deviceContext">
		/// A <see cref="DeviceContext"/> that specify the device context which has to be linked this
		/// this Render context.
		/// </param>
		/// <param name="sharedContext">
		/// A <see cref="GraphicsContext"/> that specify the render context which has to be linked this
		/// this Render context (to share resource with it).
		/// </param>
		/// <param name="version">
		/// A <see cref="KhronosVersion"/> that specify the minimum OpenGL version required to implement.
		/// </param>
		/// <exception cref="ArgumentException">
		/// Exception thrown in the case <paramref name="version"/> is different from the currently implemented by the derive,
		/// and the OpenGL extension WGL_ARB_create_context_profile or WGL_ARB_create_context are not implemented.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown in the case <paramref name="version"/> specify a forward compatible version (greater than or equal to
		/// <see cref="GLVersion.Version_3_2"/>), and the OpenGL extension WGL_ARB_create_context_profile or WGL_ARB_create_context
		/// are not implemented.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown in the case <paramref name="deviceContext"/> is <see cref="IntPtr.Zero"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// This exception is thrown in the case it's not possible to create a valid OpenGL context.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown if <paramref name="sharedContext"/> is not null and it was created by a thread different from the calling one.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown if <paramref name="sharedContext"/> is not null and it is disposed.
		/// </exception>
		public GraphicsContext(DeviceContext deviceContext, GraphicsContext sharedContext, KhronosVersion version) :
			this(deviceContext, sharedContext, version, GraphicsContextFlags.None)
		{

		}

		#endregion

		/// <summary>
		/// Construct a GraphicsContext specifying the implemented OpenGL version.
		/// </summary>
		/// <param name="deviceContext">
		/// A <see cref="DeviceContext"/> that specify the device context which has to be linked this
		/// this Render context.
		/// </param>
		/// <param name="sharedContext">
		/// A <see cref="GraphicsContext"/> that specify the render context which has to be linked this
		/// this Render context (to share resource with it).
		/// </param>
		/// <param name="version">
		/// A <see cref="KhronosVersion"/> that specify the minimum OpenGL version required to implement.
		/// </param>
		/// <param name="flags">
		/// A <see cref="GraphicsContextFlags"/> that specify special features to enable in the case they are supported.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Exception thrown if <paramref name="deviceContext"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Exception thrown in the case <paramref name="version"/> is different from the currently implemented by the derive,
		/// and the OpenGL extension WGL_ARB_create_context_profile or WGL_ARB_create_context are not implemented.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown in the case <paramref name="version"/> specify a forward compatible version (greater than or equal to
		/// <see cref="GLVersion.Version_3_2"/>), and the OpenGL extension WGL_ARB_create_context_profile or WGL_ARB_create_context
		/// are not implemented.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown in the case <paramref name="devctx"/> is <see cref="IntPtr.Zero"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// This exception is thrown in the case it's not possible to create a valid OpenGL context.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown if <paramref name="sharedContext"/> is not null and it was created by a thread different from the calling one.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// This exception is thrown if <paramref name="sharedContext"/> is not null and it is disposed.
		/// </exception>
		public GraphicsContext(DeviceContext deviceContext, GraphicsContext sharedContext, KhronosVersion version, GraphicsContextFlags flags)
		{
			try {
				IntPtr sharedContextHandle = (sharedContext != null) ? sharedContext._RenderContext : IntPtr.Zero;

#if DEBUG
				_ConstructorStackTrace = Environment.StackTrace;
#endif

				// Store thread ID of the render context
				_RenderContextThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
				// Store thread ID of the device context
				_DeviceContextThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

				if (deviceContext == null)
					throw new ArgumentNullException("deviceContext");
				if ((sharedContext != null) && (sharedContext._DeviceContext == null))
					throw new ArgumentException("shared context disposed", "hSharedContext");
				if ((sharedContext != null) && (sharedContext._RenderContextThreadId != _RenderContextThreadId))
					throw new ArgumentException("shared context created from another thread", "hSharedContext");
				if ((version != null) && (version != Gl.CurrentVersion) && ((Gl.PlatformExtensions.CreateContext_ARB == false) && (Gl.PlatformExtensions.CreateContextProfile_ARB == false)))
					throw new ArgumentException("unable to specify OpenGL version when GL_ARB_create_context[_profile] is not supported");

				// Store device context handle
				_DeviceContext = deviceContext;
				_DeviceContext.IncRef();

				// Allow version to be null (fallback to current version)
				version = version ?? Gl.CurrentVersion;
				// Set flags
				_ContextFlags = flags;

				if ((version.Api == KhronosVersion.ApiGl) && (version >= Gl.Version_300) && (Gl.PlatformExtensions.CreateContext_ARB || Gl.PlatformExtensions.CreateContextProfile_ARB)) {
					List<int> cAttributes = new List<int>();

					#region Context Version

					// Requires a specific version
					Debug.Assert(Wgl.CONTEXT_MAJOR_VERSION_ARB == Glx.CONTEXT_MAJOR_VERSION_ARB);
					Debug.Assert(Wgl.CONTEXT_MINOR_VERSION_ARB == Glx.CONTEXT_MINOR_VERSION_ARB);
					cAttributes.AddRange(new int[] {
						Wgl.CONTEXT_MAJOR_VERSION_ARB, version.Major,
						Wgl.CONTEXT_MINOR_VERSION_ARB, version.Minor
					});

					#endregion

					#region Context Profile

					uint contextProfile = 0;

					// Check binary compatibility between WGL and GLX
					Debug.Assert(Wgl.CONTEXT_PROFILE_MASK_ARB == Glx.CONTEXT_PROFILE_MASK_ARB);
					Debug.Assert(Wgl.CONTEXT_CORE_PROFILE_BIT_ARB == Glx.CONTEXT_CORE_PROFILE_BIT_ARB);
					Debug.Assert(Wgl.CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB == Glx.CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB);
					Debug.Assert(Wgl.CONTEXT_ES_PROFILE_BIT_EXT == Glx.CONTEXT_ES_PROFILE_BIT_EXT);

					// By default, Core profile

					// Core profile?
					if ((flags & GraphicsContextFlags.CoreProfile) != 0)
						contextProfile |= Wgl.CONTEXT_CORE_PROFILE_BIT_ARB;
					// Compatibility profile?
					if ((flags & GraphicsContextFlags.CompatibilityProfile) != 0)
						contextProfile |= Wgl.CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB;
					// OpenGL ES profile?
					if ((flags & GraphicsContextFlags.EmbeddedProfile) != 0)
						contextProfile |= Wgl.CONTEXT_ES_PROFILE_BIT_EXT;

					if (contextProfile != 0) {
						cAttributes.AddRange(new int[] {
							Wgl.CONTEXT_PROFILE_MASK_ARB, unchecked((int)contextProfile)
						});
					}

					#endregion

					#region Context Flags

					uint contextFlags = 0;

					// Check binary compatibility between WGL and GLX
					Debug.Assert(Wgl.CONTEXT_FLAGS_ARB == Glx.CONTEXT_FLAGS_ARB);
					Debug.Assert(Wgl.CONTEXT_FORWARD_COMPATIBLE_BIT_ARB == Glx.CONTEXT_FORWARD_COMPATIBLE_BIT_ARB);
					Debug.Assert(Wgl.CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB == Glx.CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB);
					Debug.Assert(Wgl.CONTEXT_DEBUG_BIT_ARB == Glx.CONTEXT_DEBUG_BIT_ARB);
					Debug.Assert(Wgl.CONTEXT_ROBUST_ACCESS_BIT_ARB == Glx.CONTEXT_ROBUST_ACCESS_BIT_ARB);
					Debug.Assert(Wgl.CONTEXT_RESET_ISOLATION_BIT_ARB == Glx.CONTEXT_RESET_ISOLATION_BIT_ARB);

					if (((flags & GraphicsContextFlags.CompatibilityProfile) != 0) && (Gl.CurrentExtensions.Compatibility_ARB == false))
						throw new NotSupportedException("compatibility profile not supported");
					if (((flags & GraphicsContextFlags.Robust) != 0) && (Gl.CurrentExtensions.Robustness_ARB == false && Gl.CurrentExtensions.Robustness_EXT == false))
						throw new NotSupportedException("robust profile not supported");

					// Context flags: debug context
					if ((flags & GraphicsContextFlags.Debug) != 0)
						contextFlags |= Wgl.CONTEXT_DEBUG_BIT_ARB;
					// Context flags: forward compatible context
					if ((flags & GraphicsContextFlags.ForwardCompatible) != 0)
						contextFlags |= Wgl.CONTEXT_FORWARD_COMPATIBLE_BIT_ARB;
					// Context flags: robust behavior
					if ((flags & GraphicsContextFlags.Robust) != 0)
						contextFlags |= Wgl.CONTEXT_ROBUST_ACCESS_BIT_ARB;
					// Context flags: reset isolation
					if ((flags & GraphicsContextFlags.ResetIsolation) != 0)
						contextFlags |= Wgl.CONTEXT_RESET_ISOLATION_BIT_ARB;

					if (contextFlags != 0) {
						cAttributes.AddRange(new int[] {
							Wgl.CONTEXT_FLAGS_ARB, unchecked((int)contextFlags)
						});
					}

					#endregion

					// End of attributes
					cAttributes.Add(0);

					// Create rendering context
					int[] contextAttributes = cAttributes.ToArray();

					_RenderContext = _DeviceContext.CreateContextAttrib(sharedContextHandle, contextAttributes);
					Debug.Assert(_RenderContext != IntPtr.Zero);
				} else {
					// Create rendering context
					_RenderContext = _DeviceContext.CreateContext(sharedContextHandle);
					Debug.Assert(_RenderContext != IntPtr.Zero);
				}

				if (_RenderContext == IntPtr.Zero)
					throw new InvalidOperationException(String.Format("unable to create context {0}", version));

				if (sharedContext == null) {

					// Allow the creation of a GraphicsContext while another GraphicsContext is currently current to the
					// calling thread: restore currency after the job get done
					GraphicsContext prevContext = GetCurrentContext();
					DeviceContext prevContextDevice = (prevContext != null) ? prevContext._CurrentDeviceContext : null;

					// Make current on this thread
					MakeCurrent(deviceContext, true);

					// Get the current OpenGL and Shading Language implementation supported by this GraphicsContext
					_Version = KhronosVersion.Parse(Gl.GetString(StringName.Version));
					_ShadingVersion = KhronosVersion.Parse(Gl.GetString(StringName.ShadingLanguageVersion));
					// Reserved object name space
					_ObjectNameSpace = Guid.NewGuid();

					// Texture download
					_TextureDownloadFramebuffer = new Framebuffer();
					_TextureDownloadFramebuffer.Create(this);

					// Shader include library (GLSL #include support)
					_ShaderIncludeLibrary = new ShaderIncludeLibrary();
					_ShaderIncludeLibrary.Create(this);
					// IGraphicsResource Async methods @todo ANGLE?
					// StartAsyncResourceThread();

					// Restore previous current context, if any. Otherwise, make uncurrent
					if (prevContext != null)
						prevContext.MakeCurrent(prevContextDevice, true);
					else
						MakeCurrent(deviceContext, false);
				} else {
					// Not sure if really necessary
					_CurrentDeviceContext = sharedContext._CurrentDeviceContext;

					// Same version
					_Version = sharedContext._Version;
					_ShadingVersion = sharedContext._ShadingVersion;
					// Sharing same object name space
					_ObjectNameSpace = sharedContext._ObjectNameSpace;
					// Texture download
					_TextureDownloadFramebuffer = sharedContext._TextureDownloadFramebuffer;
					// Reference shader include library (GLSL #include support)
					_ShaderIncludeLibrary = sharedContext._ShaderIncludeLibrary;
				}
			} catch {
				// Rethrow the exception
				throw;
			}
		}

#if DEBUG
		/// <summary>
		/// Stack trace when constructor were called.
		/// </summary>
		private string _ConstructorStackTrace = String.Empty;
#endif

		#endregion

		#region OpenGL Versioning

		/// <summary>
		/// The OpenGL version implemented by this GraphicsContext.
		/// </summary>
		public KhronosVersion Version { get { return (_Version); } }

		/// <summary>
		/// The OpenGL version implemented by this GraphicsContext.
		/// </summary>
		private KhronosVersion _Version;

		/// <summary>
		/// The OpenGL Shading Language version implemented by this GraphicsContext.
		/// </summary>
		public KhronosVersion ShadingVersion { get { return (_ShadingVersion); } }

		/// <summary>
		/// The OpenGL Shading Language version implemented by this GraphicsContext.
		/// </summary>
		private KhronosVersion _ShadingVersion;

		/// <summary>
		/// Flags used to create this GraphicsContext.
		/// </summary>
		public GraphicsContextFlags Flags { get { return (_ContextFlags); } }

		/// <summary>
		/// The compatibility profile presence implemented by this GraphicsContext.
		/// </summary>
		public bool IsCompatibleProfile { get { return ((_ContextFlags & GraphicsContextFlags.CompatibilityProfile) != 0); } }

		/// <summary>
		/// Flags used to create this GraphicsContext.
		/// </summary>
		private readonly GraphicsContextFlags _ContextFlags;

		#endregion

		#region Object Namespace

		/// <summary>
		/// GraphicsContext object namespace.
		/// </summary>
		public Guid ObjectNameSpace { get { return (_ObjectNameSpace); } }

		/// <summary>
		/// Object namespace identifier.
		/// </summary>
		private Guid _ObjectNameSpace = Guid.Empty;

		#endregion

		#region Lazy Objects Binding

		/// <summary>
		/// Bind the specified resource.
		/// </summary>
		/// <param name="bindingResource">
		/// The <see cref="IBindingResource"/> to be bound on this GraphicsContext.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Exception thrown if <paramref name="bindingResource"/> is null.
		/// </exception>
		public void Bind(IBindingResource bindingResource)
		{
			if (bindingResource == null)
				throw new ArgumentNullException("bindingResource");

			WeakReference<IBindingResource> boundResourceRef;
			int bindingTarget = bindingResource.BindingTarget;

			if (bindingTarget != 0 && _BoundObjects.TryGetValue(bindingTarget, out boundResourceRef)) {
				IBindingResource boundResource;

				// It may be disposed
				boundResourceRef.TryGetTarget(out boundResource);

				if (ReferenceEquals(bindingResource, boundResource)) {
					Debug.Assert(bindingResource.IsBound(this));
					// Resource already bound, avoid rendundant "Bind" operation
					return;
				}
			}

			// Debug.Assert(!bindingResource.IsBound(this));
			bindingResource.Bind(this);

			// Remind this object as bound
			if (bindingTarget != 0)
				_BoundObjects[bindingTarget] = new WeakReference<IBindingResource>(bindingResource);
		}

		/// <summary>
		/// Unbind the specified resource XXX It should be really applicable?.
		/// </summary>
		/// <param name="bindingResource">
		/// The <see cref="IBindingResource"/> to be bound on this GraphicsContext.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Exception thrown if <paramref name="bindingResource"/> is null.
		/// </exception>
		public void Unbind(IBindingResource bindingResource)
		{
			if (bindingResource == null)
				throw new ArgumentNullException("bindingResource");

			int bindingTarget = bindingResource.BindingTarget;

			if (bindingTarget != 0) {
				Debug.Assert(bindingResource.IsBound(this));
				bool res = _BoundObjects.Remove(bindingTarget);
				Debug.Assert(res);
			}

			bindingResource.Unbind(this);
		}

		/// <summary>
		/// Map between binding points.
		/// </summary>
		private readonly Dictionary<int, WeakReference<IBindingResource>> _BoundObjects = new Dictionary<int, WeakReference<IBindingResource>>();

		#endregion

		#region Fixed Pipeline

		/// <summary>
		/// Reset the current program bound.
		/// </summary>
		public void ResetProgram()
		{
			GraphicsResource.CheckCurrentContext(this);

			if ((Gl.CurrentExtensions.VertexShader_ARB || Version >= Gl.Version_200) && _BoundObjects.ContainsKey(Gl.CURRENT_PROGRAM)) {
				Gl.UseProgram(GraphicsResource.InvalidObjectName);
				_BoundObjects.Remove(Gl.CURRENT_PROGRAM);
			}
		}

		#endregion

		#region Context Currency

		/// <summary>
		/// Set this GraphicsContext current/uncurrent on current device.
		/// </summary>
		/// <param name="flag">
		/// A <see cref="Boolean"/> that specify the currency of this GraphicsContext on the
		/// device context used to create this GraphicsContext.
		/// </param>
		/// <remarks>
		/// <para>
		/// The current device is defined as follow:
		/// - If this GraphicsContext has never been current on a thread, the current device context is the one specified at construction time. In the case
		///   this GraphicsContext was not constructed specifying a device context, the common device context is the used one. Otherwise...
		/// - The last device context used to make current this context, by calling <see cref="MakeCurrent(System.IntPtr,System.Boolean)"/> or 
		///   <see cref="MakeCurrent(System.Boolean)"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ObjectDisposedException">
		/// Exception throw if this GraphicsContext has been disposed. Once the GraphicsContext has been disposed it cannot be current again.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if this GraphicsContext cannot be made current/uncurrent on the device context specified at construction time.
		/// </exception>
		public void MakeCurrent(bool flag)
		{
			if (_CurrentDeviceContext == null && _DeviceContext == null)
				throw new ObjectDisposedException("no context associated with this GraphicsContext");
			MakeCurrent(_CurrentDeviceContext ?? _DeviceContext, flag);
		}

		/// <summary>
		/// Set this GraphicsContext current/uncurrent on device different from the one specified at construction time.
		/// </summary>
		/// <param name="deviceContext">
		/// A <see cref="DeviceContext"/> that specify the device context involved.
		/// </param>
		/// <param name="flag">
		/// A <see cref="Boolean"/> that specify the currency of this GraphicsContext on the
		/// device context <paramref name="rDevice"/>.
		/// </param>
		/// <exception cref="ArgumentException">
		/// Exception throw in the case <paramref name="rDevice"/> is <see cref="IntPtr.Zero"/>.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// Exception throw if this GraphicsContext has been disposed. Once the GraphicsContext has been disposed it cannot be current again.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if this GraphicsContext cannot be made current/uncurrent on the device context specified by <paramref name="rDevice"/>.
		/// </exception>
		public void MakeCurrent(DeviceContext deviceContext, bool flag)
		{
			if (deviceContext == null)
				throw new ArgumentNullException("deviceContext");
			if (_DeviceContext == null)
				throw new ObjectDisposedException("no context associated with this GraphicsContext");

			int threadId = Thread.CurrentThread.ManagedThreadId;

			if (flag) {
				// Make this context current on device
				if (deviceContext.MakeCurrent(_RenderContext) == false)
					throw new InvalidOperationException(String.Format("cannot make current context, {0}", new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message));

				// Cache current device context
				_CurrentDeviceContext = deviceContext;
				// Set current context on this thread (only on success)
				lock (_RenderThreadsLock) {
					_RenderThreads[threadId] = this;
				}
			} else {
				// Make this context uncurrent on device
				bool res = deviceContext.MakeCurrent(IntPtr.Zero);

				// Reset current context on this thread (even on error)
				lock (_RenderThreadsLock) {
					_RenderThreads[threadId] = null;
				}

				if (res == false)
					throw new InvalidOperationException("context cannot be uncurrent because error " + Marshal.GetLastWin32Error());
			}
		}

		/// <summary>
		/// Determine whether rendering context is current on the calling thread.
		/// </summary>
		/// <returns>
		/// It returns true if the render context is current on the calling thread, otherwise it returns false.
		/// </returns>
		public bool IsCurrent
		{
			get
			{
				int threadId = Thread.CurrentThread.ManagedThreadId;

				lock (_RenderThreadsLock) {
					GraphicsContext currentThreadContext;

					// Get context registered as current to the current thread
					if (_RenderThreads.TryGetValue(threadId, out currentThreadContext) == false)
						return (false);
					// Test identification corrispodence
					if (currentThreadContext != null)
						return (currentThreadContext._RenderContextId == _RenderContextId);
					else
						return (false);
				}
			}
		}

		/// <summary>
		/// Get the current GraphicsContext on the calling thread.
		/// </summary>
		/// <returns>
		/// It returns any GraphicsContext current on the calling thread. In the case no context is current, it returns null.
		/// </returns>
		public static GraphicsContext GetCurrentContext()
		{
			int threadId = Thread.CurrentThread.ManagedThreadId;

			lock (_RenderThreadsLock) {
				GraphicsContext currentThreadContext;

				if (_RenderThreads.TryGetValue(threadId, out currentThreadContext) == false)
					return (null);

				return (currentThreadContext);
			}
		}

		/// <summary>
		/// Flush GraphicsContext commands.
		/// </summary>
		/// <remarks>
		/// Flushing rendering operations is automatically done by the system when it is more approriate. There's no need to
		/// call it manually.
		/// </remarks>
		public void Flush()
		{
			Gl.Flush();
		}

		/// <summary>
		/// Device context handle referred by GraphicsContext at construction time.
		/// </summary>
		private DeviceContext _DeviceContext;

		/// <summary>
		/// Device context handle referred by GraphicsContext when has became current.
		/// </summary>
		/// <remarks>
		/// <para>
		/// It could be different from <see cref="_DeviceContext"/>. It will be used internally to make current this GraphicsContext
		/// on the correct device context.
		/// </para>
		/// <para>
		/// If its value is <see cref="IntPtr.Zero"/>, it means that this GraphicsContext has never been current on a thread.
		/// </para>
		/// </remarks>
		private DeviceContext _CurrentDeviceContext;

		/// <summary>
		/// Get the underlying render context handle.
		/// </summary>
		public IntPtr Handle { get { return (_RenderContext); } }

		/// <summary>
		/// Rendering context handle.
		/// </summary>
		private IntPtr _RenderContext = IntPtr.Zero;

		/// <summary>
		/// Flag indicating whether <see cref="_DeviceContext"/> is a device context for the entire screen. This means that
		/// no <see cref="GraphicsWindow"/> instance will release the device context, so this GraphicsContext instance will
		/// take care of releasing it.
		/// </summary>
		// private readonly bool _CommonDeviceContext;

		/// <summary>
		/// Thread identifier which has created the device context. This field is meaninfull only in the case <see cref="_CommonDeviceContext"/> is
		/// true.
		/// </summary>
		/// <remarks>
		/// This value is used in the case the device context is the one offered by <see cref="HiddenWindow"/>. At Dispose time, if the calling thread
		/// identifier is not the same of the value of this field, an exception will be thrown becuase device context shall be released by the same
		/// thread which has allocated it.
		/// </remarks>
		private int _DeviceContextThreadId;

		/// <summary>
		/// Thread identifier which has created this GraphicsContext.
		/// </summary>
		/// <remarks>
		/// This information is used for checking whether a context can be shared with another one: contextes must be created
		/// by the same thread. This would avoid an InvalidOperationException caused by constructors.
		/// </remarks>
		private int _RenderContextThreadId;

		/// <summary>
		/// Unique identifier of this GraphicsContext.
		/// </summary>
		private Guid _RenderContextId = Guid.NewGuid();

		/// <summary>
		/// Map between process threads and current render context.
		/// </summary>
		private static readonly Dictionary<int, GraphicsContext> _RenderThreads = new Dictionary<int, GraphicsContext>();

		/// <summary>
		/// Object used for synchronizing <see cref="_RenderThreads"/> accesses.
		/// </summary>
		private static readonly object _RenderThreadsLock = new object();

		#endregion

		#region Asynchronous IGraphicsResource

		/// <summary>
		/// Execute <see cref="IGraphicsResource.Create(GraphicsContext)"/> on the resource thread.
		/// </summary>
		/// <param name="graphicsResource"></param>
		internal void CreateAsync(IGraphicsResource graphicsResource)
		{
			if (graphicsResource == null)
				throw new ArgumentNullException("graphicsResource");

			lock (_ResourceQueueLock) {
				_ResourceQueue.Add(graphicsResource);
				_ResourceQueueSem.Release();
			}
		}

		/// <summary>
		/// Create context to be current on <see cref="_ResourceThread"/>, and start it.
		/// </summary>
		private void StartAsyncResourceThread()
		{
			// Create a GraphicsContext that will be current on the resource thread
			_ResourceContext = new GraphicsContext(_DeviceContext, this);
			// Run
			_ResourceThread = new Thread(ResourceThread);
			_ResourceThread.Name = "GraphicsContext.ResourceThread";
			_ResourceThread.Start();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		private void StopResourceThread()
		{
			if (_ResourceThread == null)
				return;

			_ResourceThreadStop = true;
			if (_ResourceThread.Join(_ResourceThreadLatency * 3) == false)
				_ResourceThread.Abort();
			_ResourceThread = null;
		}

		/// <summary>
		/// Resource thread.
		/// </summary>
		private void ResourceThread()
		{
			// Be current on this thread
			_ResourceContext.MakeCurrent(true);

			while (_ResourceThreadStop == false) {
				if (_ResourceQueueSem.Wait(_ResourceThreadLatency) == true) {
					IGraphicsResource[] graphicResources;

					// Copy current resources
					lock (_ResourceQueueLock) {
						graphicResources = _ResourceQueue.ToArray();
						_ResourceQueue.Clear();
					}

					// Create all resources
					foreach (IGraphicsResource graphicsResource in graphicResources) {
						try {
							graphicsResource.Create(_ResourceContext);
						} catch (Exception exception) {

						}
					}
				}
			}

			_ResourceContext.Dispose();
			_ResourceContext = null;
		}

		/// <summary>
		/// 
		/// </summary>
		private GraphicsContext _ResourceContext;

		/// <summary>
		/// Thread used for CPU intensive, I/O bound operations or somewhat OpenGL object management.
		/// </summary>
		private Thread _ResourceThread;

		private const int _ResourceThreadLatency = 1000;

		/// <summary>
		/// Flag used to terminate <see cref="_ResourceThread"/>.
		/// </summary>
		private bool _ResourceThreadStop;

		/// <summary>
		/// Queue of resources to be created on this <see cref="GraphicsContext"/>, in a separate thread.
		/// </summary>
		private readonly List<IGraphicsResource> _ResourceQueue = new List<IGraphicsResource>();

		/// <summary>
		/// Object used for synchronizing accesses to <see cref="_ResourceQueue"/>.
		/// </summary>
		private readonly object _ResourceQueueLock = new object();

		/// <summary>
		/// Semaphore used for synchronizing accesses to <see cref="_ResourceQueue"/>.
		/// </summary>
		private readonly SemaphoreSlim _ResourceQueueSem = new SemaphoreSlim(0, 4);

		#endregion

		#region Texture Download

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="layer"></param>
		internal Image GetTextureImage(TextureArray2d texture, uint layer)
		{
			Image textureImage;

			_TextureDownloadFramebuffer.BindDraw(this);
			_TextureDownloadFramebuffer.DetachColors();
			_TextureDownloadFramebuffer.AttachColor(0, texture, layer);

			textureImage = _TextureDownloadFramebuffer.ReadColorBuffer(this, 0, 0, 0, texture.Width, texture.Height, texture.PixelLayout);

			_TextureDownloadFramebuffer.UnbindDraw(this);

			return (textureImage);
		}

		/// <summary>
		/// Framebuffer used to support layered/3d texture data download.
		/// </summary>
		private readonly Framebuffer _TextureDownloadFramebuffer;

		#endregion

		#region Shader Include Library

		/// <summary>
		/// 
		/// </summary>
		internal ShaderIncludeLibrary IncludeLibrary { get { return (_ShaderIncludeLibrary); } }

		/// <summary>
		/// The shader include library used for compiling shaders.
		/// </summary>
		private ShaderIncludeLibrary _ShaderIncludeLibrary;

		#endregion

		#region Logging

		/// <summary>
		/// Logger of this class.
		/// </summary>
		private static readonly ILogger _Log = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region IDisposable Implementation

		/// <summary>
		/// Finalizer.
		/// </summary>
		~GraphicsContext()
		{
			Dispose(false);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting managed/unmanaged resources.
		/// </summary>
		/// <param name="disposing">
		/// </param>
		/// <remarks>
		/// This method shall be called by the same thread which has created this GraphicsContext, but only in the case the following
		/// constructors were called:
		/// - @ref GraphicsContext::GraphicsContext()
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if this GraphicsContext has not been disposed before finalization.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if it's not possible to release correctly the OpenGL context related to this GraphicsContext.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if the current thread is not the one which has constructed this GraphicsContext.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if it's not possible to release correctly the GDI device context related to this GraphicsContext.
		/// </exception>
		private void Dispose(bool disposing)
		{
			if (disposing == true) {

				// Dispose resources
				if (_DrawArrayBuffer != null)
					_DrawArrayBuffer.Dispose(this);
				if (_VertexArray != null)
					_VertexArray.Dispose(this);

				if (_ShaderIncludeLibrary != null) {
					_ShaderIncludeLibrary.Dispose(this);
					_ShaderIncludeLibrary = null;
				}

				StopResourceThread();

				// Dispose unmanaged resources
				if (_RenderContext != IntPtr.Zero) {
					if (_DeviceContext.DeleteContext(_RenderContext) == false)
						throw new InvalidOperationException("unable to release OpenGL context");
					_RenderContext = IntPtr.Zero;
				}

				if (_DeviceContextThreadId != Thread.CurrentThread.ManagedThreadId)
					throw new InvalidOperationException("disposing on a different thread context");
				_DeviceContext.DecRef();
				_DeviceContext = null;

				// Remove context from the current ones
				int threadId = 0;
				bool threadCurrentFound = false;

				lock (_RenderThreadsLock) {
					foreach (KeyValuePair<int, GraphicsContext> pair in _RenderThreads) {
						if (ReferenceEquals(pair.Value, this) == true) {
							threadId = pair.Key;
							threadCurrentFound = true;
							break;
						}
					}
				}

				if (threadCurrentFound == true)
					_RenderThreads[threadId] = null;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if it's not possible to release correctly the OpenGL context related to this GraphicsContext.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if the current thread is not the one which has constructed this GraphicsContext.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Exception throw if it's not possible to release correctly the GDI device context related to this GraphicsContext.
		/// </exception>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		#endregion
	}
}
