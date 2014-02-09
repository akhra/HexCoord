/*
The MIT License (MIT)

Copyright (c) 2014 Theodore Lief Gannon

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Settworks.Hexagons {

	/// <summary>Hexagon grid coordinate.</summary>
	/// <remarks>
	/// Uses the q,r axial system detailed at http://www.redblobgames.com/grids/hexagons/.
	/// <para>These are "pointy topped" hexagons. The q axis points right, and the r axis points up-right.
	/// When converting to and from Unity coordinates, the length of a hexagon side is 1 unit.
	/// </remarks>
	public struct HexCoord {

		/// <summary>Position on the q axis.</summary>
		public readonly int q;
		/// <summary>Position on the r axis.</summary>
		public readonly int r;

		/// <summary>Initializes a new instance of the <see cref="Settworks.Hexagons.HexCoord"/> struct.</summary>
		/// <param name="q">Position on the q axis.</param>
		/// <param name="r">Position on the r axis.</param>
		public HexCoord(int q, int r) {
			this.q = q;
			this.r = r;
		}

		/// <summary>Position on the cubic z axis.</summary>
		/// <remarks>
		/// The q,r coordinate system is derived from an x,y,z cubic system with the constraint that x + y + z = 0.
		/// Where x = q and y = r, this property derives z as <c>-q-r</c>.
		/// </remarks>
		public int Z {
			get { return -q-r; }
		}

		/// <summary>Offset x coordinate.</summary>
		/// <remarks>
		/// Offset coordinates are a common alternative for hexagons, allowing pseudo-square grid operations.
		/// Where y = r, this property represents the x coordinate as <c>q + r/2</c>.
		/// </remarks>
		public int O {
			get { return q + (r>>1); }
		}

		/// <summary>Get the Unity position of this hex.</summary>
		public Vector2 Position() {
			return q*Q_XY + r*R_XY;
		}

		/// <summary>Get the maximum absolute cubic coordinate.</summary>
		/// <remarks>In hexagonal space this is the polar radius, i.e. distance from 0,0.</remarks>
		public int AxialLength() {
			if (q == 0 && r == 0) return 0;
			if (q > 0 && r >= 0) return q + r;
			if (q <= 0 && r > 0) return (-q < r)? r: -q;
			if (q < 0) return Z;
			return (-r > q)? -r: q;
		}

		/// <summary>Get the minimum absolute cubic coordinate.</summary>
		/// <remarks>This is the number of hexagon steps from 0,0 which are not along the maximum axis.</remarks>
		public int AxialSkew() {
			if (q == 0 && r == 0) return 0;
			if (q > 0 && r >= 0) return (q < r)? q: r;
			if (q <= 0 && r > 0) return (-q < r)? Math.Min(-q, q + r): Math.Min(r, -q - r);
			if (q < 0) return (q > r)? -q: -r;
			return (-r > q)? Math.Min(q, -q -r): Math.Min(-r, q + r);
		}

		/// <summary>Get the angle from 0,0 to the center of this hex.</summary>
		public float PolarAngle() {
			Vector3 pos = Position();
			return (float)Math.Atan2(pos.y, pos.x);
		}

		/// <summary>Get the counterclockwise position of this hex in the ring at its distance from 0,0.</summary>
		public int PolarIndex() {
			if (q == 0 && r == 0) return 0;
			if (q > 0 && r >= 0) return r;
			if (q <= 0 && r > 0) return (-q < r)? r - q: -3 * q - r;
			if (q < 0) return -4 * (q + r) + q;
			return (-r > q)? -4 * r + q: 6 * q + r;
		}

		/// <summary>Get a neighboring hex.</summary>
		/// <param name="index">Index of the desired neighbor. Cyclically constrained 0..5.</param>
		/// <remarks>Neighbor 0 is to the right, others proceed counterclockwise.</remarks>
		public HexCoord Neighbor(int index) {
			return NeighborVector(index) + this;
		}

		/// <summary>Enumerate this hex's six neighbors.</summary>
		/// <param name="first">Index of the first neighbor to enumerate.</param>
		/// <remarks>Neighbor 0 is to the right, others proceed counterclockwise.</remarks>
		public IEnumerable<HexCoord> Neighbors(int first = 0) {
			foreach (HexCoord hex in NeighborVectors(first))
				yield return hex + this;
		}

		/// <summary>Get the Unity position of a corner vertex.</summary>
		/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
		/// <remarks>Corner 0 is at the upper right, others proceed counterclockwise.</remarks>
		public Vector2 Corner(int index) {
			return CornerVector(index) + Position();
		}

		/// <summary>Enumerate this hex's six corners.</summary>
		/// <param name="first">Index of the first corner to enumerate.</param>
		/// <remarks>Corner 0 is at the upper right, others proceed counterclockwise.</remarks>
		public IEnumerable<Vector2> Corners(int first = 0) {
			Vector2 pos = Position();
			foreach (Vector2 v in CornerVectors(first))
				yield return v + pos;
		}

		/// <summary>Get the polar angle to a corner vertex.</summary>
		/// <param name="index">Index of the desired corner.</param>
		/// <remarks>This is the angle in radians from the center of 0,0 to the selected corner of this hex.</remarks>
		/// <remarks>The two polar bounding corners are those whose polar angles form the widest arc.</remarks>
		public float CornerPolarAngle(int index) {
			Vector2 pos = Corner(index);
			return (float)Math.Atan2(pos.y, pos.x);
		}

		/// <summary>Get the polar angle to the clockwise bounding corner.</summary>
		/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
		/// <remarks>The two polar bounding corners are those whose polar angles form the widest arc.</remarks>
		public float PolarBoundingAngle(bool CCW = false) {
			return CornerPolarAngle(PolarBoundingCornerIndex(CCW));
		}

		/// <summary>Get the XY position of the clockwise bounding corner.</summary>
		/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
		/// <remarks>The two polar bounding corners are those whose polar angles form the widest arc.</remarks>
		public Vector2 PolarBoundingCorner(bool CCW = false) {
			return Corner(PolarBoundingCornerIndex(CCW));
		}

		/// <summary>Get the index of the clockwise bounding corner.</summary>
		/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
		/// <remarks>The two polar bounding corners are those whose polar angles form the widest arc.</remarks>
		public int PolarBoundingCornerIndex(bool CCW = false) {
			if (q == 0 && r == 0) return 0;
			if (q > 0 && r >= 0) return CCW?
				(q > r)? 1: 2:
				(q < r)? 5: 4;
			if (q <= 0 && r > 0) return (-q < r)?
				CCW?
					(r > -2 * q)? 2: 3:
					(r < -2 * q)? 0: 5:
				CCW?
					(q > -2 * r)? 3: 4:
					(q < -2 * r)? 1: 0;
			if (q < 0) return CCW?
				(q < r)? 4: 5:
				(q > r)? 2: 1;
			return (-r > q)?
				CCW?
					(r < -2 * q)? 5: 0:
					(r > -2 * q)? 3: 2:
					CCW?
					(q < -2 * r)? 0: 1:
					(q > -2 * r)? 4: 3;
		}

		/// <summary>Get the corner index of 0,0 closest to this hex's polar vector.</summary>
		public int CornerSextant() {
			if (q > 0 && r >= 0 || q == 0 && r == 0) return 0;
			if (q <= 0 && r > 0) return (-q < r)? 1: 2;
			if (q < 0) return 3;
			return (-r > q)? 4: 5;
		}

		/// <summary>Get the neighbor index of 0,0 through which this hex's polar vector passes.</summary>
		public int NeighborSextant() {
			if (q == 0 && r == 0) return 0;
			if (q > 0 && r >= 0) return (q <= r)? 1: 0;
			if (q <= 0 && r > 0) return (-q <= r)?
				(r <= -2 * q)? 2: 1:
				(q <= -2 * r)? 3: 2;
			if (q < 0) return (q >= r)? 4: 3;
			return (-r > q)?
				(r >= -2 * q)? 5: 4:
				(q >= -2 * r)? 0: 5;
		}

		/// <summary>Rotate around 0,0 in sextant increments.</summary>
		/// <param name="sextants">How many sextants to rotate by.</param>
		/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after rotation.</returns>
		public HexCoord SextantRotation(int sextants) {
			if (this == default(HexCoord)) return this;
			sextants = smod(sextants, 6);
			if (sextants == 0) return this;
			if (sextants == 1) return new HexCoord(-r, Z);
			if (sextants == 2) return new HexCoord(Z, q);
			if (sextants == 3) return new HexCoord(-q, -r);
			if (sextants == 4) return new HexCoord(r, Z);
			return new HexCoord(Z, -q);
		}

		/// <summary>Mirror across a cubic axis.</summary>
		/// <param name="axis">A corner index through which the axis passes.</param>
		/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after mirroring.</returns>
		/// <remarks>The cubic axes are "diagonal" to the hexagons, passing through two opposite corners.</remarks>
		public HexCoord Mirror(int axis = 1) {
			if (this == default(HexCoord)) return this;
			axis = smod(axis, 3);
			if (axis == 0) return new HexCoord(r, q);
			if (axis == 1) return new HexCoord(Z, r);
			return new HexCoord(q, Z);
		}

		/// <summary>Scale as a vector.</summary>
		/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after scaling.</returns>
		/// <remarks>Scaling is done in floating point, then truncated to integers.</remarks>
		public HexCoord Scale(float factor)
		{ return new HexCoord((int)(q * factor), (int)(r * factor)); }
		/// <summary>Scale as a vector.</summary>
		/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after scaling.</returns>
		public HexCoord Scale(int factor)
		{ return new HexCoord(q * factor, r * factor); }
		/// <summary>Scale as a vector.</summary>
		/// <returns><see cref="UnityEngine.Vector2"/> representing the scaled vector.</returns>
		public Vector2 ScaleToVector(float factor)
		{ return new Vector2(q * factor, r * factor); }

		/// <summary>Determines whether this hex is within a specified rectangle.</summary>
		/// <returns><c>true</c> if this instance is within the specified rectangle; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// Corners are diagonally opposite, and may be specified in any order.
		/// <para>The rectangle is represented by conversion to offset coordinates.
		/// </remarks>
		public bool IsWithinRectangle(HexCoord cornerA, HexCoord cornerB) {
			return O >= Math.Min(cornerA.O, cornerB.O)
					&& O <= Math.Max(cornerA.O, cornerB.O)
					&& r >= Math.Min(cornerA.r, cornerB.r)
					&& r <= Math.Max(cornerA.r, cornerB.r);
		}

		/// <summary>Returns a <see cref="System.String"/> that represents the current <see cref="Settworks.Hexagons.HexCoord"/>.</summary>
		/// <remarks>Matches the formatting of <see cref="UnityEngine.Vector2.ToString()"/> </remarks>
		public override string ToString () {
			return "(" + q + "," + r + ")";
		}
		
		/*
		 * Static Methods
		 */
		
		/// <summary>Distance between two hexes.</summary>
		public static int Distance(HexCoord a, HexCoord b) {
			return (a - b).AxialLength();
		}

		/// <summary>Vector from a hex to a neighbor.</summary>
		/// <param name="index">Index of the desired neighbor vector. Cyclically constrained 0..5.</param>
		/// <remarks>Neighbor 0 is to the right, others proceed counterclockwise.</remarks>
		public static HexCoord NeighborVector(int index)
		{ return neighbors[smod(index, 6)]; }

		/// <summary>Enumerate the six neighbor vectors.</summary>
		/// <param name="first">Index of the first neighbor vector to enumerate.</param>
		/// <remarks>Neighbor 0 is to the right, others proceed counterclockwise.</remarks>
		public static IEnumerable<HexCoord> NeighborVectors(int first = 0) {
			if (first == 0) {
				foreach (HexCoord hex in neighbors)
					yield return hex;
			} else {
				first = smod(first, 6);
				for (int i = first; i < 6; i++)
					yield return neighbors[i];
				for (int i = 0; i < first; i++)
			     yield return neighbors[i];
			}
		}
		
		/// <summary>Neighbor index of 0,0 through which a polar angle passes.</summary>
		public static int AngleToNeighborIndex(float angle)
		{ return (int)Math.Round(angle / SEXTANT); }
		
		/// <summary>Polar angle for a neighbor of 0,0.</summary>
		public static float NeighborIndexToAngle(int index)
		{ return index * SEXTANT; }

		/// <summary>Unity position vector from hex center to a corner.</summary>
		/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
		/// <remarks>Corner 0 is at the upper right, others proceed counterclockwise.</remarks>
		public static Vector2 CornerVector(int index) {
			return corners[smod(index, 6)];
		}

		/// <summary>Enumerate the six corner vectors.</summary>
		/// <param name="first">Index of the first corner vector to enumerate.</param>
		/// <remarks>Corner 0 is at the upper right, others proceed counterclockwise.</remarks>
		public static IEnumerable<Vector2> CornerVectors(int first = 0) {
			if (first == 0) {
				foreach (Vector2 v in corners)
					yield return v;
			} else {
				first = smod(first, 6);
				for (int i = first; i < 6; i++)
					yield return corners[i];
				for (int i = 0; i < first; i++)
					yield return corners[i];
			}
		}

		/// <summary>Corner of 0,0 closest to a polar angle.</summary>
		public static int AngleToCornerIndex(float angle)
		{ return (int)Math.Floor(angle / SEXTANT); }

		/// <summary>Polar angle for a corner of 0,0.</summary>
		public static float CornerIndexToAngle(int index)
		{ return (index + 0.5f) * SEXTANT; }
		
		/// <summary><see cref="Settworks.Hexagons.HexCoord"/> containing a Unity position.</summary>
		public static HexCoord AtPosition(Vector2 pos)
		{ return FromQRVector(VectorXYtoQR(pos)); }
		
		/// <summary><see cref="Settworks.Hexagons.HexCoord"/> from hexagonal polar coordinates.</summary>
		/// <param name="radius">Hex distance from 0,0.</param>
		/// <param name="index">Counterclockwise index.</param>
		/// <remarks>Hexagonal polar coordinates approximate a circle to a hexagonal ring.</remarks>
		public static HexCoord AtPolar(int radius, int index) {
			if (radius == 0) return default(HexCoord);
			if (radius < 0) radius = -radius;
			index = smod(index, radius * 6);
			int sextant = index / radius;
			index %= radius;
			if (sextant == 0) return new HexCoord(radius - index, index);
			if (sextant == 1) return new HexCoord(-index, radius);
			if (sextant == 2) return new HexCoord(-radius, radius - index);
			if (sextant == 3) return new HexCoord(index - radius, -index);
			if (sextant == 4) return new HexCoord(index, -radius);
			return new HexCoord(radius, index - radius);
		}

		/// <summary>Find the hexagonal polar index closest to angle at radius.</summary>
		/// <param name="radius">Hex distance from 0,0.</param>
		/// <param name="angle">Desired polar angle.</param>
		/// <remarks>Hexagonal polar coordinates approximate a circle to a hexagonal ring.</remarks>
		public static int FindPolarIndex(int radius, float angle) {
			if (radius == 0) return 0;
			return (int)Math.Round(angle * radius * 3 / Mathf.PI);
		}

		/// <summary><see cref="Settworks.Hexagons.HexCoord"/> from offset coordinates.</summary>
		/// <remarks>
		/// Offset coordinates are a common alternative for hexagons, allowing pseudo-square grid operations.
		/// <para>This conversion assumes an offset of x = q + r/2.
		/// </remarks>
		public static HexCoord AtOffset(int x, int y) {
			return new HexCoord(x - (y>>1), y);
		}

		/// <summary><see cref="Settworks.Hexagons.HexCoord"/> containing a floating-point q,r vector.</summary>
		/// <remarks>Hexagonal geometry makes normal rounding inaccurate. If working with floating-point q,r vectors, use this method to accurately convert them back to <see cref="Settworks.Hexagons.HexCoord"/>.</remarks>
		public static HexCoord FromQRVector(Vector2 QRvector) {
			float z = -QRvector.x -QRvector.y;
			int ix = (int)Math.Round(QRvector.x);
			int iy = (int)Math.Round(QRvector.y);
			int iz = (int)Math.Round(z);
			if (ix + iy + iz != 0) {
				float dx = Math.Abs(ix - QRvector.x);
				float dy = Math.Abs(iy - QRvector.y);
				float dz = Math.Abs(iz - z);
				if (dx >= dy && dx >= dz)
					ix = -iy-iz;
				else if (dy >= dz)
					iy = -ix-iz;
			}
			return new HexCoord(ix, iy);
		}

		/// <summary>Convert an x,y vector to a q,r vector.</summary>
		public static Vector2 VectorXYtoQR(Vector2 XYvector) {
			return XYvector.x*X_QR + XYvector.y*Y_QR;
		}
		
		/// <summary>Convert a q,r vector to an x,y vector.</summary>
		public static Vector2 VectorQRtoXY(Vector2 QRvector) {
			return QRvector.x*Q_XY + QRvector.y*R_XY;
		}

		/*
		 * Operators
		 */

		// Cast to Vector2 in QR space. Explicit to avoid QR/XY mix-ups.
		public static explicit operator Vector2(HexCoord h)
		{ return new Vector2(h.q, h.r); }
		// +, -, ==, !=
		public static HexCoord operator +(HexCoord a, HexCoord b)
		{ return new HexCoord(a.q+b.q, a.r+b.r); }
		public static HexCoord operator -(HexCoord a, HexCoord b)
		{ return new HexCoord(a.q-b.q, a.r-b.r); }
		public static bool operator ==(HexCoord a, HexCoord b)
		{ return a.q == b.q && a.r == b.r; }
		public static bool operator !=(HexCoord a, HexCoord b)
		{ return a.q != b.q || a.r != b.r; }
		// Mandatory overrides: Equals(), GetHashCode()
		public override bool Equals(object o)
		{ return o is HexCoord && this == (HexCoord)o; }
		public override int GetHashCode()	{
			ulong Q = (q < 0)? ((ulong)-q <<1) -1: (ulong)q <<1;
			ulong R = (r < 0)? ((ulong)-r <<1) -1: (ulong)r <<1;
			return ((Q<<32) + R).GetHashCode();
		}

		/*
		 * Internal
		 */

		// The directions array. Private to prevent overwriting elements.
		static readonly HexCoord[] neighbors = {
			new HexCoord(1, 0),
			new HexCoord(0, 1),
			new HexCoord(-1, 1),
			new HexCoord(-1, 0),
			new HexCoord(0, -1),
			new HexCoord(1, -1)
		};
		// Corner locations in XY space.
		public static readonly float SEXTANT = Mathf.PI / 3;
		static readonly Vector2[] corners = {
			new Vector2(Mathf.Sin(SEXTANT), Mathf.Cos(SEXTANT)),
			new Vector2(0, 1),
			new Vector2(Mathf.Sin(-SEXTANT), Mathf.Cos(-SEXTANT)),
			new Vector2(Mathf.Sin(Mathf.PI + SEXTANT), Mathf.Cos(Mathf.PI - SEXTANT)),
			new Vector2(0, -1),
			new Vector2(Mathf.Sin(Mathf.PI - SEXTANT), Mathf.Cos(Mathf.PI - SEXTANT))
		};
		// Vector transformations between QR and XY space.
		public static readonly float SQRT3 = Mathf.Sqrt(3);
		static readonly Vector2 Q_XY = new Vector2(SQRT3, 0);
		static readonly Vector2 R_XY = new Vector2(SQRT3/2, 1.5f);
		static readonly Vector2 X_QR = new Vector2(SQRT3/3, 0);
		static readonly Vector2 Y_QR = new Vector2(-1/3f, 2/3f);
		// Modulo where sign of result matches the divisor.
		static int smod(int a, int b)
		{ return a % b + ((a < 0 ^ b < 0)? b: 0); }

	}

	/// <summary>Serializable hexagon grid coordinate.</summary>
	/// <remarks><see cref="Settworks.Hexagons.HexCoord"/> is a struct for performance reasons, but Unity does not support serialization of structs. This serializable class is easily converted to and from <see cref="Settworks.Hexagons.HexCoord"/>, allowing it to be used in places where serialization is needed without affecting the performance of other logic.</remarks>
	[Serializable] public class HexCoordinate {
		[SerializeField] public int q,r;
		public HexCoordinate() {}
		public HexCoordinate(HexCoord h) { Become(h); }
		public HexCoord HexCoord { get { return (HexCoord)this; } }
		/// <summary>Become the specified <see cref="Settworks.Hexagons.HexCoord"/>.</summary>
		public void Become(HexCoord h) { q = h.q; r = h.r; }
		public static implicit operator HexCoord(HexCoordinate h) { return new HexCoord(h.q, h.r); }
		public static implicit operator HexCoordinate(HexCoord h) { return new HexCoordinate(h); }
	}

}