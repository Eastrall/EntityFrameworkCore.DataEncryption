using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal
{
	internal static class EncodingExtensions
	{
		public static byte[] GetBytes(this Encoding encoding, SecureString value)
		{
			if (encoding is null) throw new ArgumentNullException(nameof(encoding));
			if (value is null || value.Length == 0) return Array.Empty<byte>();

			IntPtr valuePtr = IntPtr.Zero;
			try
			{
				valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
				if (valuePtr == IntPtr.Zero) return Array.Empty<byte>();

				unsafe
				{
					char* chars = (char*)valuePtr;
					Debug.Assert(chars != null);

					int byteCount = encoding.GetByteCount(chars, value.Length);

					var result = new byte[byteCount];
					fixed (byte* bytes = result)
					{
						encoding.GetBytes(chars, value.Length, bytes, byteCount);
					}

					return result;
				}
			}
			finally
			{
				if (valuePtr != IntPtr.Zero)
				{
					Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
				}
			}
		}

		public static SecureString GetSecureString(this Encoding encoding, byte[] bytes)
		{
			if (encoding is null) throw new ArgumentNullException(nameof(encoding));
			if (bytes is null || bytes.Length == 0) return default;

			unsafe
			{
				fixed (byte* pB = bytes)
				{
					int charCount = encoding.GetCharCount(pB, bytes.Length);
					Span<char> chars = charCount < 1024 ? stackalloc char[charCount] : new char[charCount];
					fixed (char* pC = &MemoryMarshal.GetReference(chars))
					{
						charCount = encoding.GetChars(pB, bytes.Length, pC, chars.Length);
						return new SecureString(pC, charCount);
					}
				}
			}
		}
	}
}