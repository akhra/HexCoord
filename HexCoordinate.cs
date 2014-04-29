using UnityEngine;
using System;

namespace Settworks.Hexagons {

	/// <summary>Serializable hexagon grid coordinate.</summary>
	/// <remarks><see cref="Settworks.Hexagons.HexCoord"/> is a struct for performance reasons, but Unity does not support serialization of structs. This serializable class is easily converted to and from <see cref="Settworks.Hexagons.HexCoord"/>, allowing it to be used in places where serialization is needed without affecting the performance of other logic.</remarks>
	[Serializable]
	public class HexCoordinate {
		[SerializeField]
		public int q;
		[SerializeField]
		public int r;

		/// <summary>
		/// The equivalent <see cref="Settworks.Hexagons.HexCoord"/>.
		/// </summary>
		public HexCoord HexCoord {
			get {
				return (HexCoord)this;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Settworks.Hexagons.HexCoordinate"/> class.
		/// </summary>
		public HexCoordinate() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Settworks.Hexagons.HexCoordinate"/> class
		/// duplicating the Q and R values of a specified <see cref="Settworks.Hexagons.HexCoord"/>.
		/// </summary>
		/// <param name="hex"><see cref="Settworks.Hexagons.HexCoord"/> to duplicate.</param>
		public HexCoordinate(HexCoord hex) {
			Become(hex);
		}

		/// <summary>
		/// Change the Q and R values of this <see cref="Settworks.Hexagons.HexCoordinate"/> to those
		/// of the specified <see cref="Settworks.Hexagons.HexCoord"/>.
		/// </summary>
		/// <param name="hex"><see cref="Settworks.Hexagons.HexCoord"/> to become.</param>
		public void Become(HexCoord hex) {
			q = hex.q;
			r = hex.r;
		}

		public bool Is(HexCoord hex) {
			return this.q == hex.q && this.r == hex.r;
		}

		public static implicit operator HexCoord(HexCoordinate hex) {
			return new HexCoord(hex.q, hex.r);
		}

		public static implicit operator HexCoordinate(HexCoord hex) {
			return new HexCoordinate(hex);
		}
	}
}