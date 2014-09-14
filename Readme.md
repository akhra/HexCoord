HexCoord implements hexagonal grid geometry in C#. The main fork utilizes Unity's Vector2 and Vector3 for position information, but is otherwise platform-agnostic; porting to other C# environments should be trivial.

This is an implementation of [Amit Patel's constrained cubic axial coordinates](http://www.redblobgames.com/grids/hexagons/). Briefly, it uses two axes labeled q and r. The q axis is equivalent to the x axis of a Cartesian plane; but the r axis is 60 degrees from q, rather than the 90 degrees of a Cartesian y axis. HexCoord deviates from Amit's reference example in one way: the positive r axis extends up and right, rather than down and right.

For performance reasons, HexCoord is a struct. Because Unity did not serialize structs prior to version 4.5.0, there is also a serializable HexCoordinate class which easily converts to and from HexCoord. Use the class where serialization is needed in older Unity versions, and the struct everywhere else.

####Built-in operations include:
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
- Fast hashing with no collisions until coordinates exceed 15 bits + sign.

HexCoord is a key component of the non-free HexKit package, and is fully described in HexKit's [documentation](http://settworks.com/images/pdf/HexKit%20Documentation.pdf). Discussion and modest support at http://www.settworks.com/forum/hexkit
