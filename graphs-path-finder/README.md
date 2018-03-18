# PacMan/Maze Graph PathFinder

This is a basic Graph data structure implementation written in Java that finds the shortest path from the start to the end of a maze.
It represents a maze text input file as a graph, then performs a breadth-first search for the shortest path.
This program was written during my first semester as a Master's student in the MSD program in Fall 2017.

### Program Specifications:
- Maze input (.txt) files are in the following form:
```
5 5
XXXXX
XS  X
X   X
X  GX
XXXXX
```
- The first line contains the dimensions (height, width) of the maze.
- The remainder of the input file illustrates the layout of the field.
- 'X' characters represent single wall segments.
- 'S' represents the starting location of PacMan.
- 'G' represents the ending location of PacMan.
- ' ' (space) characters represent spaces that can be traversed.
- Input mazes are rectangular, and the field is fully enclosed.

## Program Output
- The output returned by the program is similar to the input:
```
5 5
XXXXX
XS..X
X  .X
X  GX
XXXXX
```
- If no path exists from starting to ending location, the output file will be identical to the input file.
