﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace dnSpy.Contracts.Hex {
	/// <summary>
	/// A stream used by a <see cref="HexBuffer"/>
	/// </summary>
	public abstract class HexBufferStream {
		/// <summary>
		/// Constructor
		/// </summary>
		protected HexBufferStream() { }

		/// <summary>
		/// true if the content can change at any time
		/// </summary>
		public virtual bool IsVolatile => false;

		/// <summary>
		/// true if it's a read-only stream
		/// </summary>
		public virtual bool IsReadOnly => false;

		/// <summary>
		/// Gets the span
		/// </summary>
		public abstract HexSpan Span { get; }

		/// <summary>
		/// Gets the name. This could be the filename if the data was read from a file
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Tries to read a <see cref="byte"/>. If there's no data, a value less than 0 is returned.
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract int TryReadByte(HexPosition position);

		/// <summary>
		/// Reads a <see cref="byte"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract byte ReadByte(HexPosition position);

		/// <summary>
		/// Reads a <see cref="sbyte"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract sbyte ReadSByte(HexPosition position);

		/// <summary>
		/// Reads a <see cref="short"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract short ReadInt16(HexPosition position);

		/// <summary>
		/// Reads a <see cref="ushort"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract ushort ReadUInt16(HexPosition position);

		/// <summary>
		/// Reads a <see cref="int"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract int ReadInt32(HexPosition position);

		/// <summary>
		/// Reads a <see cref="uint"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract uint ReadUInt32(HexPosition position);

		/// <summary>
		/// Reads a <see cref="long"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract long ReadInt64(HexPosition position);

		/// <summary>
		/// Reads a <see cref="ulong"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract ulong ReadUInt64(HexPosition position);

		/// <summary>
		/// Reads a <see cref="float"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract float ReadSingle(HexPosition position);

		/// <summary>
		/// Reads a <see cref="double"/>
		/// </summary>
		/// <param name="position">Position</param>
		/// <returns></returns>
		public abstract double ReadDouble(HexPosition position);

		/// <summary>
		/// Reads bytes
		/// </summary>
		/// <param name="position">Position</param>
		/// <param name="length">Number of bytes to read</param>
		/// <returns></returns>
		public abstract byte[] ReadBytes(HexPosition position, long length);

		/// <summary>
		/// Reads bytes
		/// </summary>
		/// <param name="position">Position</param>
		/// <param name="destination">Destination array</param>
		/// <param name="destinationIndex">Index</param>
		/// <param name="length">Length</param>
		public abstract void ReadBytes(HexPosition position, byte[] destination, long destinationIndex, long length);

		/// <summary>
		/// Reads bytes
		/// </summary>
		/// <param name="position">Position</param>
		/// <param name="length">Length</param>
		/// <returns></returns>
		public abstract HexBytes ReadHexBytes(HexPosition position, long length);

		/// <summary>
		/// Writes bytes
		/// </summary>
		/// <param name="position">Position</param>
		/// <param name="source">Data</param>
		/// <param name="sourceIndex">Index</param>
		/// <param name="length">Length</param>
		public abstract void Write(HexPosition position, byte[] source, long sourceIndex, long length);
	}
}
