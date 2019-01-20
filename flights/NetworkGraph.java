/**
 * 
 */
package airtravel;

import java.io.FileInputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.PriorityQueue;
import java.util.Scanner;

/**
 * This class represents a graph of flights and airports along with specific
 * data about those flights. It is recommended to create an airport class and a
 * flight class to represent nodes and edges respectively. There are other ways
 * to accomplish this and you are free to explore those.
 */
public class NetworkGraph {
  private final double INFINITY = Double.MAX_VALUE;
  private Map<String, LinkedList<Flight>> vertexmap; //Hashmap of outgoing flights for each airport
  private Map<String, Airport> airportmap;
  private static String filepath;
  private Map<Flight, Double> duplicateflights;

  /**
   * Inner class representing VERTICES (ie nodes).
   */
  private class Airport implements Comparable<Airport>{
    String code;
    Airport cameFrom; //previous vertex on shortest path
    boolean visited;
    List<Flight> outgoing; //For each vertex, keep a list of all adjacent vertices
    double distance;

    private Airport(String aircode) {
      code = aircode;
      outgoing = new LinkedList<Flight>();
      distance = INFINITY;
      cameFrom = null;
      visited = false;
    }

    public int compareTo(Airport o) {
      return distance < o.distance ? -1 : distance > o.distance ? 1 : 0;
    }
    private List<Flight> outgoingFlights(){
      return outgoing;
    }
    private void addOutgoing(Flight f) {
      outgoing.add(f);
    }
    private void removeOutgoing(Flight f) {
      outgoing.remove(f);
    }
  }

  /**
   * Inner class representing EDGES connecting the vertices
   * of the network graph. Each edge pair is ordered (i.e., network
   * graph is a directed graph. 
   */
  private class Flight{
    private int duplicatecount;
    private double weight;
    private Airport goingTo;
    private Airport cameFrom;
    private String carrier;

    Flight(Airport source, Airport destination, double cost){
      goingTo = destination;
      weight = cost;
      duplicatecount = 1;
      cameFrom = source;
    }

    boolean equals(Flight flight) {
      return this.goingTo.equals(flight.goingTo) && this.cameFrom.equals(flight.cameFrom);
    }

  }
  /**
   * Constructs a NetworkGraph object and populates it with the information
   * contained in the given file.
   * 
   * @param flightInfoPath - path to the .csv file containing flight data.
   * @throws IOException 
   */
  public NetworkGraph(String flightInfoPath) throws IOException {
    vertexmap = new HashMap<String, LinkedList<Flight>>();
    filepath = flightInfoPath;
    airportmap = new HashMap<String, Airport>();
    duplicateflights = new HashMap<Flight, Double>();

  }

  private void readData(int filterby, String carrier) throws IOException {
    try {
      Scanner s = new Scanner(new FileInputStream(filepath));
      if (!filepath.contains(".csv")) {
        System.err.println("Error: Invalid File Format. File must be a .csv file");
      }
      while(s.hasNext()) {
        s.nextLine();
        while (s.hasNextLine()) {
          String[] info = s.nextLine().split(",");
          if (info.length == 8) {
            if (carrier == null || carrier.equals(info[2])) {
              addEdge(info[0], info[1], Double.parseDouble(info[filterby]));
            }
          }
        }
      }
      for (Flight each: duplicateflights.keySet()) { //add duplicate flight entries to vertexmap
        updateFlightinMap(each);
      }
    }
    catch(IOException e) {
      System.err.println(e);
    }
  }

  private void addEdge(String source, String dest, double cost) {
    Airport from = getAirport(source);
    Airport to = getAirport(dest);
    assert airportmap.containsValue(from) && airportmap.containsValue(to);
    Flight edge = new Flight(from, to, cost);
    boolean found = false;
    Iterator<Flight> it = from.outgoingFlights().iterator();
    while (it.hasNext()) {
      if (it.next().equals(edge)) {
        found = true;
        break;
      } else continue;
    }
    if(!found) {
      from.addOutgoing(edge);
    }
//    for (Flight each:from.outgoingFlights()) {
////      System.out.println(each.cameFrom.code + " TO " + each.goingTo.code);
//      if (each.equals(edge)) {
//        found = true;
//        break;
////        from.removeOutgoing(each);
//      }
//    }
//    from.addOutgoing(edge);
//    }
    boolean hasflight = false;
    assert vertexmap.get(source) != null;
    for (Flight flight: vertexmap.get(source)) {
      if (flight.equals(edge)) {
        hasflight = true;
        flight.duplicatecount++;
        if (!duplicateflights.containsKey(flight)) {
          duplicateflights.put(flight, flight.weight + cost);
        } 
        else if (duplicateflights.containsKey(flight)) {
          duplicateflights.put(flight, duplicateflights.get(flight) + cost); 
        }   
      }
    }
    if (vertexmap.get(source) != null && !hasflight) {
      vertexmap.get(source).add(edge);
    }
    
  }

  private void updateFlightinMap(Flight flight) {
    vertexmap.get(flight.cameFrom.code).remove(flight);
    flight.cameFrom.removeOutgoing(flight);
    Flight updated = new Flight(flight.cameFrom, flight.goingTo, averageData(flight));
    flight.cameFrom.addOutgoing(updated);
    vertexmap.get(flight.cameFrom.code).add(updated);
  }
  private double averageData(Flight flight) {
    return duplicateflights.get(flight)/flight.duplicatecount;
  }
  private Airport getAirport(String airport) {
    if (!vertexmap.containsKey(airport)) {
      vertexmap.put(airport, new LinkedList<Flight>());
    }
    if (!airportmap.containsKey(airport)) {
      Airport air = new Airport(airport);
      airportmap.put(airport, air);
      return air;
    }
    return airportmap.get(airport);
  }

  /**
   * Im
   * @param origin
   * @param dest
   */
  private ArrayList<String> getPath(Airport origin, Airport dest){
    ArrayList<String> bestpath = new ArrayList<String>();
    if (dest.distance == INFINITY) {
      System.err.println("There is no path to your chosen destination");
    } else {
      bestpath.add(dest.code);
      while (dest.cameFrom != null && dest != origin) {
        bestpath.add(0,dest.cameFrom.code);
        dest = dest.cameFrom;
      }
    }
    return bestpath;
  }

  private void dijkstrasAlgorithm(Airport start, Airport goal) {
    PriorityQueue<Airport> queue = new PriorityQueue<>();
    start.distance = 0; //distance from start to start is 0
    queue.add(start);
    while (!queue.isEmpty()) {
      Airport currentNode = queue.remove(); 
      if (currentNode.equals(goal)) {
        return;
      }
      currentNode.visited = true; //Mark visited after removal from queue
      for (Flight flight: currentNode.outgoingFlights()) {
        Airport nextstop = flight.goingTo;
        if (!nextstop.visited && 
            nextstop.distance > currentNode.distance + flight.weight) { // ie shorter path found
          nextstop.cameFrom = currentNode;
          nextstop.distance = currentNode.distance + flight.weight; // update path cost ie update priority in queue 
          queue.add(nextstop); //enqueue next airport with updated cost
        }
      }
    }
  }
  /**
   * This method returns a BestPath object containing information about the best
   * way to fly to the destination from the origin. "Best" is defined by the
   * FlightCriteria parameter <code>enum</code>. 
   * 
   * @param origin -a String denoting the origin airport code.
   * @param destination - a String denoting the destination airport code.
   * @param criteria - enumeration dictating the criteria to be used for "best".
   * @return - An object containing path information including origin,
   *         destination, and everything in between.
   * @throws IOException 
   */
  public BestPath getBestPath(String origin, String destination, FlightCriteria criteria) throws IOException {
    int index = getHeaderIndex(criteria);
    readData(index, null);
    Airport dest = getAirport(destination);
    Airport source = getAirport(origin);
    dijkstrasAlgorithm(source, dest);
    BestPath shortest = new BestPath(getPath(source,dest), dest.distance);
    return shortest;
  }
  private int getHeaderIndex(FlightCriteria criteria) {
    int column = 0;
    switch(criteria) {
    case DELAY:
      column = 3;
      break;
    case CANCELED:
      column = 4;
      break;
    case TIME:
      column = 5;
      break;
    case DISTANCE:
      column = 6;
      break;
    case COST:
      column = 7;
      break;
    default:
      break;
    }
    return column;
  }
  /**
   * <p>
   * This overloaded method should do the same as the one above, only restricted to 
   * a specific airliner.
   * </p>
   * 
   * @param origin
   * @param destination
   * @param criteria
   * @param airliner
   * @return - An object containing path information including origin,
   *         destination, and everything in between.
   * @throws IOException 
   */
  public BestPath getBestPath(String origin, String destination, FlightCriteria criteria, String airliner) throws IOException {
    int index = getHeaderIndex(criteria);
    readData(index, airliner);
    Airport dest = getAirport(destination);
    Airport source = getAirport(origin);
    dijkstrasAlgorithm(source, dest);
    BestPath shortest = new BestPath(getPath(source,dest), dest.distance);
    return shortest;
  }


}
