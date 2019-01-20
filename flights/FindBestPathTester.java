package airtravel;

import java.io.IOException;

/**
 * <p>
 * An example of how a user will use your best flight API.
 * </p>
 */
public class FindBestPathTester {

  public static void main(String[] args) throws IOException {
    NetworkGraph airportGraph = null;
    String filepath = "./resources/testfile.csv";
    String test = "flights.csv";
    try {
      airportGraph = new NetworkGraph(filepath);
    } catch (Exception e) {
      e.printStackTrace();
    }

//    // Returns the shortest distance path of flights from MOB to ACV
//    // Solution: a path of ['MOB', 'DFW', 'SFO', 'ACV'] and distance of 2253
//    BestPath shortestDistancePath = airportGraph.getBestPath("MOB", "ACV", FlightCriteria.DISTANCE);
//    System.out.println(shortestDistancePath.toString());

    // Returns the shortest distance path of flights from SFO to DWF when flying
    // with DL
    // Solution: a path of ['SFO', 'SLC', 'DFW'] and distance of 1588
    BestPath shortestDistancePath2 = airportGraph.getBestPath("UEN", "NBU", FlightCriteria.DISTANCE);
//    System.out.println(shortestDistancePath2.toString());
//    BestPath shortestDistancePath2 = airportGraph.getBestPath("SFO", "DFW", FlightCriteria.DISTANCE);
    System.out.println(shortestDistancePath2.toString());

    // Returns the shortest flight time path from MOB to SLC
    // Solution: a path of ['MOB', 'DFW', 'SLC'] and time of ~269.25
//    BestPath shortestTimePath = airportGraph.getBestPath("MOB", "SLC", FlightCriteria.TIME);
//    System.out.println(shortestTimePath.toString());

    // Returns the fiscally cheapest path of flights from LAS to LAX
    // Solution: a path of ['LAS', 'LAX'] and cost of ~138.39
//    BestPath cheapestPath = airportGraph.getBestPath("LAS", "LAX", FlightCriteria.COST);
//    System.out.println(cheapestPath.toString());
  }

}
