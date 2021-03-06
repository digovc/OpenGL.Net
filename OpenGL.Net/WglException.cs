﻿
// Copyright (C) 2015 Luca Piccioni
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
using System.ComponentModel;

namespace OpenGL
{
	/// <summary>
	/// Exception thrown by Wgl class.
	/// </summary>
	class WglException : KhronosException
	{
		#region Constructors

		/// <summary>
		/// Construct a GlException.
		/// </summary>
		/// <param name="errorCode">
		/// A <see cref="ErrorCode"/> that specifies the error code.
		/// </param>
		public WglException(int errorCode) :
			base(errorCode, GetErrorMessage(errorCode), new Win32Exception(errorCode))
		{

		}

		/// <summary>
		/// Construct a GlException.
		/// </summary>
		/// <param name="errorCode">
		/// A <see cref="ErrorCode"/> that specifies the error code.
		/// </param>
		/// <param name="message">
		/// A <see cref="String"/> that specifies the exception message.
		/// </param>
		public WglException(int errorCode, String message) :
			base(errorCode, message, new Win32Exception(errorCode))
		{

		}

		/// <summary>
		/// Construct a GlException.
		/// </summary>
		/// <param name="errorCode">
		/// A <see cref="ErrorCode"/> that specifies the error code.
		/// </param>
		/// <param name="message">
		/// A <see cref="String"/> that specifies the exception message.
		/// </param>
		/// <param name="innerException">
		/// The <see cref="Exception"/> wrapped by this Exception.
		/// </param>
		public WglException(int errorCode, String message, Exception innerException) :
			base(errorCode, message, innerException)
		{

		}

		#endregion

		#region Error Messages

		/// <summary>
		/// Returns a description of the error code.
		/// </summary>
		/// <param name="errorCode"></param>
		/// <returns>
		/// It returns a description of <paramref name="errorCode"/>, asssuming that is a value returned
		/// by <see cref="Gl.GetError"/>.
		/// </returns>
		private static string GetErrorMessage(int errorCode)
		{
			switch (errorCode) {

				default:
					return (String.Format("unknown error code 0x{0}", errorCode.ToString("X8")));
				case Gl.NO_ERROR:
					return ("no error");

				// WGL_ARB_create_context
				case Wgl.ERROR_INVALID_VERSION_ARB:
					return ("invalid version");
				// WGL_ARB_create_context_profile
				case Wgl.ERROR_INVALID_PROFILE_ARB:
					return ("invalid profile");
				// WGL_ARB_make_current_read
				case Wgl.ERROR_INVALID_PIXEL_TYPE_ARB:
					return ("invalid pixel type");
				case Wgl.ERROR_INCOMPATIBLE_DEVICE_CONTEXTS_ARB:
					return ("incompatible device contexts");
				// WGL_NV_gpu_affinity
				case Wgl.ERROR_INCOMPATIBLE_AFFINITY_MASKS_NV:
					return ("incompatible affinity mask");
				case Wgl.ERROR_MISSING_AFFINITY_MASK_NV:
					return ("missing affinity mask");
			}
		}

		#endregion
	}
}
