/**
 * 
 */
package airtravel;

import java.util.ArrayList;

public class BestPath {

  /**
   * This should contain the nodes between the origin and destination inclusive.
   * For example if I want the path between SLC and MEM it should have SLC (index
   * 0), MEM (index 1). If there are lay overs it should include them in between
   * (turns out you can fly to Memphis from here directly).
   */
  public ArrayList<String> path;

  /**
   * Since some path costs are going to be doubles sometimes use a double when
   * costs are integers cast to a double.
   */
  public double pathLength;

  BestPath(ArrayList<String> bestpath, double length){
    path = bestpath;
    pathLength = length;
  }
  @Override
  public boolean equals(Object o) {
    if (o instanceof BestPath) {
      BestPath other = (BestPath) o;
      return this.pathLength == other.pathLength && this.path.equals(other.path);
    }
    return false;
  }

  @Override
  public String toString() {
    return "Path Length: " + pathLength + "\nPath: " + this.path;
  }
}
