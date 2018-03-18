package assignment08;

import java.util.List;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.Queue;

public class Graph {
  private Node[][] storage;
  private Node startNode;
  private Node endNode;

  /**
   * Graph Constructor: initializes all (x,y) coordinates
   * within the maze, adds neighbors for each char in the
   * maze, then calls breadthFirstSearch() to 
   * generate a solution to the maze.  
   * @param rows
   * @param cols
   * @param maze
   */
  Graph(int rows, int cols, char[] maze) {
    storage = new Node[rows][cols];
    for (int i= 0, k = 0; i < rows; i++) {
      for (int j = 0; j < cols; j++, k++) {
        storage[i][j] = new Node(maze[k]);
        switch(maze[k]) {
        case 'X':
          storage[i][j] = null;
          break;
        case 'S': 
          startNode = storage[i][j];
          break;
        case 'G':
          endNode = storage[i][j];
          break;
        default:
          storage[i][j] = new Node(' ');
        }
      }
    }
    for (int i= 1; i < rows-1; i++) {
      for (int j = 1; j < cols-1; j++) {
        if (storage[i][j] != null) {
          storage[i][j].addNeighbors(i, j);
        }
      }
    }
  }
  /**
   * Helper method that utilizes BFS traversal and a queue data 
   * structure to find the shortest path from startNode to endNode.
   */
  public void breadthFirstSearch() {
    Queue<Node> queue = new LinkedList<>();
    startNode.visited = true;
    queue.add(startNode);  
    while(!queue.isEmpty()) {
      Node current = queue.remove();
      if (current.equals(endNode)) {
        break;
      }
      for (Node node: current.neighbors) {
        if (node != null && !node.visited) {
          node.visited = true;
          node.cameFrom = current;
          queue.add(node);
        }
      }
    }
    if (endNode.visited) { //Check if a solution was found
      solveMaze(); 
    }
  }

  /**
   * Helper method in solving maze that retraces and marks
   * the path traversed from endNode to startNode using
   * '.' characters. 
   */
  private void solveMaze(){
    Node current = endNode;
    while (current.cameFrom != null && current.cameFrom != startNode) {
      current.cameFrom.data = '.';
      current = current.cameFrom; 
    }
  }

  /**
   * Helper method for converting the maze solution to a char[].
   * @param rows
   * @param cols
   * @return
   */
  public char[] getSolution(int rows, int cols){
    char[] solved =  new char[rows * (cols + 1)];
        int k = 0;
    for (int i= 0; i < rows; i++) {
      for (int j = 0; j <= cols; j++, k++) {
        if (j == cols) {
          solved[k] = '\n';
          continue;
        }
        if (storage[i][j] == null) {
          solved[k] = 'X';
        } else {
          solved[k] = storage[i][j].data;
        }
      }
    }
    return solved;
  }
  /**
   * Inner Node class for designating vertices of the 
   * maze's graph data structure.
   */
  private class Node{
    char data;
    List<Node> neighbors;
    Node cameFrom;
    boolean visited;

    private Node(char element) {
      data = element;
      visited = false;
      cameFrom = null;
      neighbors = null;
      neighbors = new ArrayList<>();
    }
    /**
     * Helper method for initializing and retrieving neighbors
     * for a given node.
     * @param x  -- row number (i.e., y-coordinate)
     * @param y  -- column number (i.e., x-coordinate)
     */
    private void addNeighbors(int x, int y) {
      if (storage[x-1][y] != null) {
        neighbors.add(storage[x-1][y]);
      }
      if (storage[x+1][y] != null) {
        neighbors.add(storage[x+1][y]);
      }
      if (storage[x][y+1] != null) {
        neighbors.add(storage[x][y+1]);
      }
      if (storage[x][y-1] != null) {
        neighbors.add(storage[x][y-1]);
      }
    }
  }

}
