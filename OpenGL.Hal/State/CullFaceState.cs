
// Copyright (C) 2010-2015 Luca Piccioni
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
using System.Diagnostics;

namespace OpenGL.State
{
	/// <summary>
	/// Culling face state
	/// </summary>
	public class CullFaceState : GraphicsState
	{
		#region Constructors
		
		/// <summary>
		/// Construct a default CullFaceState (front face is CCW, culling backfaces).
		/// </summary>
		public CullFaceState()
		{
			
		}

		/// <summary>
		/// Construct a CullFaceState, specifying front face and disabling culling.
		/// </summary>
		/// <param name="frontFace">
		/// A <see cref="FrontFace"/> that determine how front faces are determined.
		/// </param>
		public CullFaceState(FrontFaceDirection frontFace)
		{
			_FrontFaceMode = frontFace;
			_Enabled = false;
		}

		/// <summary>
		/// Construct a CullFaceState, enabling front/back face culling (front face is defaulted to CCW).
		/// </summary>
		/// <param name="culledFace">
		/// A <see cref="CullFaceMode"/> that specify which faces are culled.
		/// </param>
		public CullFaceState(CullFaceMode culledFace)
			: this(FrontFaceDirection.Ccw, culledFace)
		{
			
		}

		/// <summary>
		/// Construct a CullFaceState, specifying front face and enabling culling of the specified faces.
		/// </summary>
		/// <param name="frontFace">
		/// A <see cref="FrontFaceDirection"/> that specify how front faces are determined.
		/// </param>
		/// <param name="culledFace">
		/// A <see cref="CullFaceMode"/> that specify which faces are culled.
		/// </param>
		public CullFaceState(FrontFaceDirection frontFace, CullFaceMode culledFace)
		{
			_FrontFaceMode = frontFace;
			_Enabled = true;
			_CulledFace = culledFace;
		}

		/// <summary>
		/// Construct the current CullFaceState.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> defining this GraphicsState.
		/// </param>
		public CullFaceState(GraphicsContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException("ctx");

			int frontFace, cullFaceMode;

			// Determine how front face is determined
			Gl.Get(GetPName.FrontFace, out frontFace);
			_FrontFaceMode = (FrontFaceDirection)frontFace;

			// Determine whether face culling is enabled
			_Enabled = Gl.IsEnabled(EnableCap.CullFace);

			// Determine which faces are culled
			Gl.Get(GetPName.CullFaceMode, out cullFaceMode);
			_CulledFace = (CullFaceMode)cullFaceMode;
		}
		
		#endregion
		
		#region Cull Face State Definition

		/// <summary>
		/// Front face determination mode property.
		/// </summary>
		public FrontFaceDirection FrontFaceMode
		{
			get { return (_FrontFaceMode); }
			set { _FrontFaceMode = value; }
		}

		/// <summary>
		/// Cull enabled flag.
		/// </summary>
		public bool Culling
		{
			get { return (_Enabled); }
			set { _Enabled = value; }
		}

		/// <summary>
		/// Face culled.
		/// </summary>
		public CullFaceMode CulledFace
		{
			get { return (_CulledFace); }
			set { _CulledFace = value; }
		}

		/// <summary>
		/// Front face determination
		/// </summary>
		private FrontFaceDirection _FrontFaceMode = FrontFaceDirection.Ccw;

		/// <summary>
		/// Enabled flag.
		/// </summary>
		private bool _Enabled = true;

		/// <summary>
		/// Face culled.
		/// </summary>
		private CullFaceMode _CulledFace = CullFaceMode.Back;
		
		#endregion

		#region Default State

		/// <summary>
		/// The system default state for CullFaceState.
		/// </summary>
		public static CullFaceState DefaultState { get { return (new CullFaceState()); } }

		#endregion
		
		#region GraphicsState Overrides

		/// <summary>
		/// The name of this render context.
		/// </summary>
		public static readonly string StateId = "OpenGL.CullFace";

		/// <summary>
		/// The identifier of this GraphicsState.
		/// </summary>
		public override string StateIdentifier { get { return (StateId); } }
		
		/// <summary>
		/// Set ShaderProgram state.
		/// </summary>
		/// <param name="ctx">
		/// A <see cref="GraphicsContext"/> which has defined the shader program <paramref name="sProgram"/>.
		/// </param>
		/// <param name="sProgram">
		/// The <see cref="ShaderProgram"/> which has the state set.
		/// </param>
		public override void ApplyState(GraphicsContext ctx, ShaderProgram sProgram)
		{
			if (ctx == null)
				throw new ArgumentNullException("ctx");

			// Front face determination
			Gl.FrontFace(_FrontFaceMode);
			// Culling
			if (_Enabled) {
				// Face culling
				Gl.Enable(EnableCap.CullFace);
				// Culled face
				Gl.CullFace(_CulledFace);
			} else {
				//No face culling
				Gl.Disable(EnableCap.CullFace);
			}
		}

		/// <summary>
		/// Merge this state with another one.
		/// </summary>
		/// <param name="state">
		/// A <see cref="IGraphicsState"/> having the same <see cref="StateIdentifier"/> of this state.
		/// </param>
		public override void Merge(IGraphicsState state)
		{
			if (state == null)
				throw new ArgumentNullException("state");

			CullFaceState otherState = state as CullFaceState;

			if (otherState == null)
				throw new ArgumentException("not a CullFaceState", "state");

			_FrontFaceMode = otherState._FrontFaceMode;
			_Enabled = otherState._Enabled;
			_CulledFace = otherState._CulledFace;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">
		/// A <see cref="GraphicsState"/> to compare to this GraphicsState.
		/// </param>
		/// <returns>
		/// It returns true if the current object is equal to <paramref name="other"/>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This method test only whether <paramref name="other"/> type equals to this type.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// This exception is thrown if the parameter <paramref name="other"/> is null.
		/// </exception>
		public override bool Equals(IGraphicsState other)
		{
			if (base.Equals(other) == false)
				return (false);
			Debug.Assert(other is CullFaceState);

			CullFaceState otherState = (CullFaceState) other;

			if (otherState._FrontFaceMode != _FrontFaceMode)
				return (false);
			if (otherState._Enabled != _Enabled)
				return (false);
			if (otherState._CulledFace != _CulledFace)
				return (false);

			return (true);
		}
		
		/// <summary>
		/// Represents the current <see cref="GraphicsState"/> for logging.
		/// </summary>
		/// <returns>
		/// A <see cref="String"/> that represents the current <see cref="GraphicsState"/>.
		/// </returns>
		public override string ToString()
		{
			if (_Enabled)
				return (String.Format("{0}: FrontFaceMode={1} CulledFace={2} (Disabled)", StateIdentifier, FrontFaceMode, CulledFace));
			else
				return (String.Format("{0}: FrontFaceMode={1} CulledFace={2}", StateIdentifier, FrontFaceMode, CulledFace));
		}

		#endregion
	}
}
