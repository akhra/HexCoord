HexCoord implements hexagonal grid geometry in C#. The main fork utilizes Unity's Vector2 and Vector3 for position information, but is otherwise platform-agnostic; porting to other C# environments should be trivial.

This is an implementation of Amit Patel's constrained cubic axial coordinates, described at http://www.redblobgames.com/grids/hexagons/. Briefly, it uses two axes labeled q and r. The q axis is equivalent to the x axis of a Cartesian plane; but the r axis is 60 degrees from q, rather than the 90 degrees of a Cartesian y axis. HexCoord deviates from Amit's reference example in one way: the positive r axis extends up and right, rather than down and right.

For performance reasons, HexCoord is a struct. Because Unity does not serialize structs, there is also a serializable HexCoordinate class which easily converts to and from HexCoord. Use the class where serialization is needed, and the struct everywhere else.

Built-in operations include:
- Conversion to and from Unity positions.
- Conversion to and from offset-Cartesian coordinates.
- Hexagonal grid polar coordinates (radius, position on ring).
- Conversion to and from real polar coordinates and numerous related functions.
- Indexed access to the six adjacent hexagons.
- Indexed corners, including Unity position of corners.
- Rotate through sextants (1/6 circle)
- Mirror across the three diagonals.
- Hexagonal Manhattan (grid-step) distance.
- Vector addition, subtraction and scaling.
- Robust hashing, or so I'm told.

Discussion and modest support at http://www.settworks.com/forum/