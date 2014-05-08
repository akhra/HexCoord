using UnityEngine;
using System;
using System.Collections.Generic;

namespace Settworks.Hexagons {
	/// <summary>
	/// Hexagon grid coordinate.
	/// </summary>
	/// <remarks>
	/// Uses the q,r axial system detailed at http://www.redblobgames.com/grids/hexagons/.
	/// These are "pointy topped" hexagons. The q axis points right, and the r axis points up-right.
	/// When converting to and from Unity coordinates, the length of a hexagon side is 1 unit.
	/// </remarks>
	public struct HexCoord {

		/// <summary>
		/// Position on the q axis.
		/// </summary>
		public readonly int q;
		/// <summary>
		/// Position on the r axis.
		/// </summary>
		public readonly int r;

		/// <summary>
		/// Initializes a new instance of the <see cref="Settworks.Hexagons.HexCoord"/> struct.
		/// </summary>
		/// <param name="q">Position on the q axis.</param>
		/// <param name="r">Position on the r axis.</param>
		public HexCoord(int q, int r) {
			this.q = q;
			this.r = r;
		}

		/// <summary>
		/// Position on the cubic z axis.
		/// </summary>
		/// <remarks>
		/// The q,r coordinate system is derived from an x,y,z cubic system with the constraint that x + y + z = 0.
		/// Where x = q and y = r, this property derives z as <c>-q-r</c>.
		/// </remarks>
		public int Z {
			get { return -q-r; }
		}

		/// <summary>
		/// Offset x coordinate.
		/// </summary>
		/// <remarks>
		/// Offset coordinates are a common alternative for hexagons, allowing pseudo-square grid operations.
		/// Where y = r, this property represents the x coordinate as <c>q + r/2</c>.
		/// </remarks>
		public int O {
			get { return q + (r>>1); }
		}

		/// <summary>
		/// Unity position of this hex.
		/// </summary>
		public Vector2 Position() {
			return q*Q_XY + r*R_XY;
		}

		/// <summary>
		/// Get the maximum absolute cubic coordinate.
		/// </summary>
		/// <remarks>
		/// In hexagonal space this is the polar radius, i.e. distance from 0,0.
		/// </remarks>
		public int AxialLength() {
			if (q == 0 && r == 0) return 0;
			if (q > 0 && r >= 0) return q + r;
			if (q <= 0 && r > 0) return (-q < r)? r: -q;
			if (q < 0) return Z;
			return (-r > q)? -r: q;
		}

		/// <summary>
		/// Get the minimum absolute cubic coordinate.
		/// </summary>
		/// <remarks>
		/// This is the number of hexagon steps from 0,0 which are not along the maximum axis.
		/// </remarks>
		public int AxialSkew() {
			if (q == 0 && r == 0) return 0;
			if (q > 0 && r >= 0) return (q < r)? q: r;
			if (q <= 0 && r > 0) return (-q < r)? Math.Min(-q, q + r): Math.Min(r, -q - r);
			if (q < 0) return (q > r)? -q: -r;
			return (-r > q)? Math.Min(q, -q -r): Math.Min(-r, q + r);
		}

		/// <summary>
		/// Get the angle from 0,0 to the center of this hex.
		/// </summary>
		public float PolarAngle() {
			Vector3 pos = Position();
			return (float)Math.Atan2(pos.y, pos.x);
		}

		/// <summary>
		/// Get the counterclockwise position of this hex in the ring at its distance from 0,0.
		/// </summary>
		public int PolarIndex() {
			if (q == 0 && r == 0) return 0;
			if (q > 0 && r >= 0) return r;
			if (q <= 0 && r > 0) return (-q < r)? r - q: -3 * q - r;
			if (q < 0) return -4 * (q + r) + q;
			return (-r > q)? -4 * r + q: 6 * q + r;
		}

		/// <summary>
		/// Get a neighboring hex.
		/// </summary>
		/// <remarks>
		/// Neighbor 0 is to the right, others proceed counterclockwise.
		/// </remarks>
		/// <param name="index">Index of the desired neighbor. Cyclically constrained 0..5.</param>
		public HexCoord Neighbor(int index) {
			return NeighborVector(index) + this;
		}

		/// <summary>
		/// Enumerate this hex's six neighbors.
		/// </summary>
		/// <remarks>
		/// Neighbor 0 is to the right, others proceed counterclockwise.
		/// </remarks>
		/// <param name="first">Index of the first neighbor to enumerate.</param>
		public IEnumerable<HexCoord> Neighbors(int first = 0) {
			foreach (HexCoord hex in NeighborVectors(first))
				yield return hex + this;
		}

		/// <summary>
		/// Get the Unity position of a corner vertex.
		/// </summary>
		/// <remarks>
		/// Corner 0 is at the upper right, others proceed counterclockwise.
		/// </remarks>
		/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
		public Vector2 Corner(int index) {
			return CornerVector(index) + Position();
		}

		/// <summary>
		/// Enumerate this hex's six corners.
		/// </summary>
		/// <remarks>
		/// Corner 0 is at the upper right, others proceed counterclockwise.
		/// </remarks>
		/// <param name="first">Index of the first corner to enumerate.</param>
		public IEnumerable<Vector2> Corners(int first = 0) {
			Vector2 pos = Position();
			foreach (Vector2 v in CornerVectors(first))
				yield return v + pos;
		}

		/// <summary>
		/// Get the polar angle to a corner vertex.
		/// </summary>
		/// <remarks>
		/// This is the angle in radians from the center of 0,0 to the selected corner of this hex.
		/// </remarks>
		/// <param name="index">Index of the desired corner.</param>
		public float CornerPolarAngle(int index) {
			Vector2 pos = Corner(index);
			return (float)Math.Atan2(pos.y, pos.x);
		}

		/// <summary>
		/// Get the polar angle to the clockwise bounding corner.
		/// </summary>
		/// <remarks>
		/// The two polar bounding corners are those whose polar angles form the widest arc.
		/// </remarks>
		/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
		public float PolarBoundingAngle(bool CCW = false) {
			return CornerPolarAngle(PolarBoundingCornerIndex(CCW));
		}

		/// <summary>
		/// Get the XY position of the clockwise bounding corner.
		/// </summary>
		/// <remarks>
		/// The two polar bounding corners are those whose polar angles form the widest arc.
		/// </remarks>
		/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
		public Vector2 PolarBoundingCorner(bool CCW = false) {
			return Corner(PolarBoundingCornerIndex(CCW));
		}

		/// <summary>
		/// Get the index of the clockwise bounding corner.
		/// </summary>
		/// <remarks>
		/// The two polar bounding corners are those whose polar angles form the widest arc.
		/// </remarks>
		/// <param name="CCW">If set to <c>true</c>, gets the counterclockwise bounding corner.</param>
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

		/// <summary>
		/// Get the half sextant of origin containing this hex.
		/// </summary>
		/// <remarks>
		/// CornerSextant is HalfSextant/2. NeighborSextant is (HalfSextant+1)/2.
		/// </remarks>
		public int HalfSextant() {
			if (q > 0 && r >= 0 || q == 0 && r == 0)
				return (q > r)? 0 : 1;
			if (q <= 0 && r > 0)
				return (-q < r)?
					(r > -2 * q)? 2: 3:
					(q > -2 * r)? 4: 5;
			if (q < 0)
				return (q < r)? 6: 7;
			return (-r > q)?
				(r < -2 * q)? 8: 9:
				(q < -2 * r)? 10: 11;
		} 

		/// <summary>
		/// Get the corner index of 0,0 closest to this hex's polar vector.
		/// </summary>
		public int CornerSextant() {
			if (q > 0 && r >= 0 || q == 0 && r == 0) return 0;
			if (q <= 0 && r > 0) return (-q < r)? 1: 2;
			if (q < 0) return 3;
			return (-r > q)? 4: 5;
		}

		/// <summary>
		/// Get the neighbor index of 0,0 through which this hex's polar vector passes.
		/// </summary>
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

		/// <summary>
		/// Rotate around 0,0 in sextant increments.
		/// </summary>
		/// <returns>
		/// A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after rotation.
		/// </returns>
		/// <param name="sextants">How many sextants to rotate by.</param>
		public HexCoord SextantRotation(int sextants) {
			if (this == origin) return this;
			sextants = NormalizeRotationIndex(sextants, 6);
			if (sextants == 0) return this;
			if (sextants == 1) return new HexCoord(-r, -Z);
			if (sextants == 2) return new HexCoord(Z, q);
			if (sextants == 3) return new HexCoord(-q, -r);
			if (sextants == 4) return new HexCoord(r, Z);
			return new HexCoord(-Z, -q);
		}

		/// <summary>
		/// Mirror across a cubic axis.
		/// </summary>
		/// <remarks>
		/// The cubic axes are "diagonal" to the hexagons, passing through two opposite corners.
		/// </remarks>
		/// <param name="axis">A corner index through which the axis passes.</param>
		/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after mirroring.</returns>
		public HexCoord Mirror(int axis = 1) {
			if (this == origin) return this;
			axis = NormalizeRotationIndex(axis, 3);
			if (axis == 0) return new HexCoord(r, q);
			if (axis == 1) return new HexCoord(Z, r);
			return new HexCoord(q, Z);
		}

		/// <summary>
		/// Scale as a vector, truncating result.
		/// </summary>
		/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after scaling.</returns>
		public HexCoord Scale(float factor)
		{ return new HexCoord((int)(q * factor), (int)(r * factor)); }
		/// <summary>
		/// Scale as a vector.
		/// </summary>
		/// <returns>A new <see cref="Settworks.Hexagons.HexCoord"/> representing this one after scaling.</returns>
		public HexCoord Scale(int factor)
		{ return new HexCoord(q * factor, r * factor); }
		/// <summary>
		/// Scale as a vector.
		/// </summary>
		/// <returns><see cref="UnityEngine.Vector2"/> representing the scaled vector.</returns>
		public Vector2 ScaleToVector(float factor)
		{ return new Vector2(q * factor, r * factor); }

		/// <summary>
		/// Determines whether this hex is within a specified rectangle.
		/// </summary>
		/// <returns><c>true</c> if this instance is within the specified rectangle; otherwise, <c>false</c>.</returns>
		public bool IsWithinRectangle(HexCoord cornerA, HexCoord cornerB) {
			if (r > cornerA.r && r > cornerB.r || r < cornerA.r && r < cornerB.r)
				return false;
			bool reverse = cornerA.O > cornerB.O;	// Travel right to left.
			bool offset = cornerA.r % 2 != 0;	// Starts on an odd row, bump alternate rows left.
			bool trim = Math.Abs(cornerA.r - cornerB.r) % 2 == 0;	// Even height, trim alternate rows.
			bool odd = (r - cornerA.r) % 2 != 0; // This is an alternate row.
			int width = Math.Abs(cornerA.O - cornerB.O);
			bool hasWidth = width != 0;
			if (reverse && (odd && (trim || !offset) || !(trim || offset || odd))
			    || !reverse && (trim && odd || offset && !trim && hasWidth))
				width -= 1;
			int x = (O - cornerA.O) * (reverse? -1: 1);
			if (reverse && odd && !offset
			    || !reverse && offset && odd && hasWidth)
				x -= 1;
			return (x <= width && x >= 0);
		}

		/// <summary>
		/// Determines whether this hex is on the infinite line passing through points a and b.
		/// </summary>
		public bool IsOnCartesianLine(Vector2 a, Vector2 b) {
			Vector2 AB = b - a;
			bool bias = Vector3.Cross(AB, Corner(0) - a).z > 0;
			for (int i = 1; i < 6; i++) {
				if (bias != (Vector3.Cross(AB, Corner(i) - a).z > 0))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether this the is on the line segment between points a and b.
		/// </summary>
		public bool IsOnCartesianLineSegment(Vector2 a, Vector2 b) {
			Vector2 AB = b - a;
			float mag = AB.sqrMagnitude;
			Vector2 AC = Corner(0) - a;
			bool within = AC.sqrMagnitude <= mag && Vector2.Dot(AB, AC) >= 0;
			int sign = Math.Sign(Vector3.Cross(AB, AC).z);
			for (int i = 1; i < 6; i++) {
				AC = Corner(i) - a;
				bool newWithin = AC.sqrMagnitude <= mag && Vector2.Dot(AB, AC) >= 0;
				int newSign =	Math.Sign(Vector3.Cross(AB, AC).z);
				if ((within || newWithin) && (sign * newSign <= 0))
					return true;
				within = newWithin;
				sign = newSign;
			}
			return false;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Settworks.Hexagons.HexCoord"/>.
		/// </summary>
		/// <remarks>
		/// Matches the formatting of <see cref="UnityEngine.Vector2.ToString()"/>.
		/// </remarks>
		public override string ToString () {
			return "(" + q + "," + r + ")";
		}
		
		/*
		 * Static Methods
		 */

		/// <summary>
		/// HexCoord at (0,0)
		/// </summary>
		public static readonly HexCoord origin = default(HexCoord);
		
		/// <summary>
		/// Distance between two hexes.
		/// </summary>
		public static int Distance(HexCoord a, HexCoord b) {
			return (a - b).AxialLength();
		}

		/// <summary>
		/// Normalize a rotation index within 0 <= index < cycle.
		/// </summary>
		public static int NormalizeRotationIndex(int index, int cycle = 6) {
			if (index < 0 ^ cycle < 0)
				return (index % cycle + cycle) % cycle;
			else
				return index % cycle;
		}

		/// <summary>
		/// Determine the equality of two rotation indices for a given cycle.
		/// </summary>
		public static bool IsSameRotationIndex(int a, int b, int cycle = 6) {
			return 0 == NormalizeRotationIndex(a - b, cycle);
		}

		/// <summary>
		/// Vector from a hex to a neighbor.
		/// </summary>
		/// <remarks>
		/// Neighbor 0 is to the right, others proceed counterclockwise.
		/// </remarks>
		/// <param name="index">Index of the desired neighbor vector. Cyclically constrained 0..5.</param>
		public static HexCoord NeighborVector(int index)
		{ return neighbors[NormalizeRotationIndex(index, 6)]; }

		/// <summary>
		/// Enumerate the six neighbor vectors.
		/// </summary>
		/// <remarks>
		/// Neighbor 0 is to the right, others proceed counterclockwise.
		/// </remarks>
		/// <param name="first">Index of the first neighbor vector to enumerate.</param>
		public static IEnumerable<HexCoord> NeighborVectors(int first = 0) {
			if (first == 0) {
				foreach (HexCoord hex in neighbors)
					yield return hex;
			} else {
				first = NormalizeRotationIndex(first, 6);
				for (int i = first; i < 6; i++)
					yield return neighbors[i];
				for (int i = 0; i < first; i++)
			     yield return neighbors[i];
			}
		}
		
		/// <summary>
		/// Neighbor index of 0,0 through which a polar angle passes.
		/// </summary>
		public static int AngleToNeighborIndex(float angle)
		{ return (int)Math.Round(angle / SEXTANT); }
		
		/// <summary>
		/// Polar angle for a neighbor of 0,0.
		/// </summary>
		public static float NeighborIndexToAngle(int index)
		{ return index * SEXTANT; }

		/// <summary>
		/// Unity position vector from hex center to a corner.
		/// </summary>
		/// <remarks>
		/// Corner 0 is at the upper right, others proceed counterclockwise.
		/// </remarks>
		/// <param name="index">Index of the desired corner. Cyclically constrained 0..5.</param>
		public static Vector2 CornerVector(int index) {
			return corners[NormalizeRotationIndex(index, 6)];
		}

		/// <summary>
		/// Enumerate the six corner vectors.
		/// </summary>
		/// <remarks>
		/// Corner 0 is at the upper right, others proceed counterclockwise.
		/// </remarks>
		/// <param name="first">Index of the first corner vector to enumerate.</param>
		public static IEnumerable<Vector2> CornerVectors(int first = 0) {
			if (first == 0) {
				foreach (Vector2 v in corners)
					yield return v;
			} else {
				first = NormalizeRotationIndex(first, 6);
				for (int i = first; i < 6; i++)
					yield return corners[i];
				for (int i = 0; i < first; i++)
					yield return corners[i];
			}
		}

		/// <summary>
		/// Corner of 0,0 closest to a polar angle.
		/// </summary>
		public static int AngleToCornerIndex(float angle)
		{ return (int)Math.Floor(angle / SEXTANT); }

		/// <summary>
		/// Polar angle for a corner of 0,0.
		/// </summary>
		public static float CornerIndexToAngle(int index)
		{ return (index + 0.5f) * SEXTANT; }
		
		/// <summary>
		/// <see cref="Settworks.Hexagons.HexCoord"/> containing a Unity position.
		/// </summary>
		public static HexCoord AtPosition(Vector2 position)
		{ return FromQRVector(VectorXYtoQR(position)); }
		
		/// <summary>
		/// <see cref="Settworks.Hexagons.HexCoord"/> from hexagonal polar coordinates.
		/// </summary>
		/// <remarks>
		/// Hexagonal polar coordinates approximate a circle to a hexagonal ring.
		/// </remarks>
		/// <param name="radius">Hex distance from 0,0.</param>
		/// <param name="index">Counterclockwise index.</param>
		public static HexCoord AtPolar(int radius, int index) {
			if (radius == 0) return origin;
			if (radius < 0) radius = -radius;
			index = NormalizeRotationIndex(index, radius * 6);
			int sextant = index / radius;
			index %= radius;
			if (sextant == 0) return new HexCoord(radius - index, index);
			if (sextant == 1) return new HexCoord(-index, radius);
			if (sextant == 2) return new HexCoord(-radius, radius - index);
			if (sextant == 3) return new HexCoord(index - radius, -index);
			if (sextant == 4) return new HexCoord(index, -radius);
			return new HexCoord(radius, index - radius);
		}

		/// <summary>
		/// Find the hexagonal polar index closest to angle at radius.
		/// </summary>
		/// <remarks>
		/// Hexagonal polar coordinates approximate a circle to a hexagonal ring.
		/// </remarks>
		/// <param name="radius">Hex distance from 0,0.</param>
		/// <param name="angle">Desired polar angle.</param>
		public static int FindPolarIndex(int radius, float angle) {
			if (radius == 0) return 0;
			return (int)Math.Round(angle * radius * 3 / Mathf.PI);
		}

		/// <summary>
		/// <see cref="Settworks.Hexagons.HexCoord"/> from offset coordinates.
		/// </summary>
		/// <remarks>
		/// Offset coordinates are a common alternative for hexagons, allowing pseudo-square grid operations.
		/// This conversion assumes an offset of x = q + r/2.
		/// </remarks>
		public static HexCoord AtOffset(int x, int y) {
			return new HexCoord(x - (y>>1), y);
		}

		/// <summary>
		/// <see cref="Settworks.Hexagons.HexCoord"/> containing a floating-point q,r vector.
		/// </summary>
		/// <remarks>
		/// Hexagonal geometry makes normal rounding inaccurate. If working with floating-point
		/// q,r vectors, use this method to accurately convert them back to
		/// <see cref="Settworks.Hexagons.HexCoord"/>.
		/// </remarks>
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

		/// <summary>
		/// Convert an x,y vector to a q,r vector.
		/// </summary>
		public static Vector2 VectorXYtoQR(Vector2 XYvector) {
			return XYvector.x*X_QR + XYvector.y*Y_QR;
		}
		
		/// <summary>
		/// Convert a q,r vector to an x,y vector.
		/// </summary>
		public static Vector2 VectorQRtoXY(Vector2 QRvector) {
			return QRvector.x*Q_XY + QRvector.y*R_XY;
		}

		/// <summary>
		/// Get the corners of a QR-space rectangle containing every cell touching an XY-space rectangle.
		/// </summary>
		public static HexCoord[] CartesianRectangleBounds(Vector2 cornerA, Vector2 cornerB) {
			Vector2 min = new Vector2(Math.Min(cornerA.x, cornerB.x), Math.Min(cornerA.y, cornerB.y));
			Vector2 max = new Vector2(Math.Max(cornerA.x, cornerB.x), Math.Max(cornerA.y, cornerB.y));
			HexCoord[] results = {
				HexCoord.AtPosition(min),
				HexCoord.AtPosition(max)
			};
			Vector2 pos = results[0].Position();
			if ((pos + corners[0]).y <= min.y || (pos + corners[5]).y >= min.y)
				results[0] += neighbors[4];
			else if ((pos + corners[1]).x <= min.x)
				results[0] += neighbors[3];
			pos = results[1].Position();
			if ((pos + corners[2]).y <= max.y || (pos + corners[3]).y >= max.y)
				results[1] += neighbors[1];
			else if ((pos + corners[1]).x >= max.x)
				results[1] += neighbors[0];
			return results;
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
		{ return (o is HexCoord) && this == (HexCoord)o; }
		public override int GetHashCode() {
			return q&1 | (r&1)<<1 | (q&2)<<1 | (r&2)<<2 |
			       (q&4)<<2 | (r&4)<<3 | (q&8)<<3 | (r&8)<<4 |
			       (q&16)<<4 | (r&16)<<5 | (q&32)<<5 | (r&32)<<6 |
			       (q&64)<<6 | (r&64)<<7 | (q&128)<<7 | (r&128)<<8 |
			       (q&256)<<8 | (r&256)<<9 | (q&512)<<9 | (r&512)<<10 |
			       (q&1024)<<10 | (r&1024)<<11 | (q&2048)<<11 | (r&2048)<<12 |
			       (q&4096)<<12 | (r&4096)<<13 | (q&8192)<<13 | (r&8192)<<14 |
			       (q&16384)<<14 | (r&16384)<<15 | (q&32768)<<15 | (r&32768)<<16;
		}

		/*
		 * Constants
		 */

		
		/// <summary>
		/// One sixth of a full rotation (radians).
		/// </summary>
		public static readonly float SEXTANT = Mathf.PI / 3;
		
		/// <summary>
		/// Square root of 3.
		/// </summary>
		public static readonly float SQRT3 = Mathf.Sqrt(3);

		// The directions array. These are private to prevent overwriting elements.
		static readonly HexCoord[] neighbors = {
			new HexCoord(1, 0),
			new HexCoord(0, 1),
			new HexCoord(-1, 1),
			new HexCoord(-1, 0),
			new HexCoord(0, -1),
			new HexCoord(1, -1)
		};

		// Corner locations in XY space. Private for same reason as neighbors.
		static readonly Vector2[] corners = {
			new Vector2(Mathf.Sin(SEXTANT), Mathf.Cos(SEXTANT)),
			new Vector2(0, 1),
			new Vector2(Mathf.Sin(-SEXTANT), Mathf.Cos(-SEXTANT)),
			new Vector2(Mathf.Sin(Mathf.PI + SEXTANT), Mathf.Cos(Mathf.PI - SEXTANT)),
			new Vector2(0, -1),
			new Vector2(Mathf.Sin(Mathf.PI - SEXTANT), Mathf.Cos(Mathf.PI - SEXTANT))
		};

		// Vector transformations between QR and XY space.
		// Private to keep IntelliSense tidy. Safe to make public, but sensible uses are covered above.
		static readonly Vector2 Q_XY = new Vector2(SQRT3, 0);
		static readonly Vector2 R_XY = new Vector2(SQRT3/2, 1.5f);
		static readonly Vector2 X_QR = new Vector2(SQRT3/3, 0);
		static readonly Vector2 Y_QR = new Vector2(-1/3f, 2/3f);

	}
}