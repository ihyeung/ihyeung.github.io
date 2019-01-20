/**
 *
 */
package airtravel;

/**
 * <p>
 * FlightCriteria are used to designate what type of "best" is looked for while
 * path finding. Usage of enumerations are very simple; see the java
 * documentation.<br>
 * <p>
 *
 * Your code should support the following flight path criteria.
 *
 * <li>{@link #COST}</li>
 * <li>{@link #DELAY}</li>
 * <li>{@link #DISTANCE}</li>
 * <li>{@link #CANCELED}</li>
 * <li>{@link #TIME}</li>
 */
public enum FlightCriteria {
  /**
   * Designates the shortest path by cost of the flight.
   */
  COST,

  /**
   * Designates the shortest path by delay of departure.
   */
  DELAY,

  /**
   * Designates the shortest path by distance IE: stops.
   */
  DISTANCE,

  /**
   * Designates the path with the least chance of being canceled.
   */
  CANCELED,

  /**
   * Designates the path with shortest average actual flight time.
   */
  TIME
}
