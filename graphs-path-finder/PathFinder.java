package assignment08;

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.Scanner;


public class PathFinder {
  private static int rows;
  private static int columns;
  private static Graph maze;
  
  /**
   * Maze Solver Method- generates a solution to a maze using
   * a graph data structure implementing breadth-first search 
   * traversal.
   * @param inputFile
   * @param outputFile
   * @throws IOException
   */
  public static void solveMaze(String inputFile, String outputFile) throws IOException  {
    char[] graph = getMaze(inputFile);
    maze = new Graph(rows, columns, graph);
    maze.breadthFirstSearch();
    createOutput(maze.getSolution(rows,columns), outputFile);
  }
  
  /**
   * Extracts maze from a .txt file and returns it as a char[].
   * @param inputFile
   * @return char[] -- the maze represented as an array of chars.
   * @throws IOException
   */
  public static char[] getMaze(String inputFile) throws IOException{
    Scanner s = null;
    try {
      s = new Scanner(new FileInputStream(inputFile));
    } catch(FileNotFoundException e) {
      System.out.println(e.getMessage());
    }
    char[] chararray = null;
    while (s.hasNext()) {
      String[] dimensions = s.nextLine().split(" ");
      rows = Integer.parseInt(dimensions[0]);
      columns = Integer.parseInt(dimensions[1]); 
      chararray = new char[rows * columns];
      int currentindex = 0;
      while (s.hasNextLine()) {
        char[] chars = s.nextLine().toCharArray();
        for (int i = currentindex, j= 0; j < chars.length; i++,j++) {
          chararray[i] = chars[j];
          currentindex++;
        }
      }
    }
    return chararray;
  }
 /**
  * Method that writes solution to maze to a .txt file.
  * @param solution -- an array of chars representing the solved maze.
  * @param outputFile -- name of file to output solution to.
  * @throws IOException 
  */
  public static void createOutput(char[] solution, String outputFile) throws IOException {
    try {
    PrintWriter pw = new PrintWriter(new FileWriter(outputFile));
    pw.println(rows + " " + columns);
    pw.print(solution);
    pw.close();
    } catch(IOException e) {
      System.out.println(e.getMessage());
    }
  }
}
