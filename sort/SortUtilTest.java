package assignment04;

import static org.junit.Assert.*;

import java.awt.List;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.Random;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

public class SortUtilTest {
  StringComparator stringcomparator;
  IntegerComparator integercomparator;
  DoubleComparator doublecomparator;
  
  @Before
  public void setUp() throws Exception {
   stringcomparator = new StringComparator();
    integercomparator = new IntegerComparator();
    doublecomparator = new DoubleComparator();
  }

  @After
  public void tearDown() throws Exception {
  }

  @Test
  public void testMergeSortBest() {
    ArrayList<Integer> large = SortUtil.generateBestCase(10000);
    ArrayList<Integer> small = SortUtil.generateBestCase(2);
    ArrayList<Integer> one = SortUtil.generateBestCase(1);
    ArrayList<String> ascwithdupe = new ArrayList<String>(Arrays.asList("a","b","c","d","e","f","g","h","h", "i"));
    ArrayList<Double> average = new ArrayList<Double>();
    for (double i = 3; i < 100; i+=3) {
      average.add(i);
    }
    assertTrue(isSorted(average, doublecomparator));
    SortUtil.mergesort(ascwithdupe, stringcomparator);
    SortUtil.mergesort(average, doublecomparator);
    SortUtil.mergesort(large, integercomparator);
    SortUtil.mergesort(small, integercomparator);
    SortUtil.mergesort(one, integercomparator);
    assertTrue(isSorted(large,integercomparator));
    assertTrue(isSorted(average,doublecomparator));
    assertTrue(isSorted(small,integercomparator));
    assertTrue(isSorted(one,integercomparator));
 
  }
  @Test
  public void testMergeSortAverage() {
    Random r = new Random();
    ArrayList<Integer> large = SortUtil.generateAverageCase(10000);
    ArrayList<Integer> average = SortUtil.generateAverageCase(15);
    ArrayList<Integer> small = SortUtil.generateAverageCase(2);
    ArrayList<Integer> one = SortUtil.generateAverageCase(1);
    ArrayList<String> avgrandst = new ArrayList<String>(); 
    for (int i = 50; i > 0; i--) {
      avgrandst.add(generateRandomString(r.nextInt(10)+ 5));
    }
    SortUtil.mergesort(avgrandst,stringcomparator);
    SortUtil.mergesort(average, integercomparator);
    SortUtil.mergesort(small, integercomparator);
    SortUtil.mergesort(one, integercomparator);
    SortUtil.mergesort(large, integercomparator);
    assertTrue(isSorted(large, integercomparator));
    assertTrue(isSorted(avgrandst, stringcomparator));
    assertTrue(isSorted(average,integercomparator));
    assertTrue(isSorted(small,integercomparator));
    assertTrue(isSorted(one,integercomparator));
  }
  @Test
  public void testMergeSortWorst() {
    ArrayList<Integer> large = SortUtil.generateWorstCase(10000);
    ArrayList<Integer> average = SortUtil.generateWorstCase(101);
    ArrayList<Integer> small = SortUtil.generateWorstCase(2);
    ArrayList<Integer> one = SortUtil.generateWorstCase(1);
    ArrayList<String> descstring = new ArrayList<String>(Arrays.asList("zoo", "yoyo", "xylophone", "walrus", "van", "umbrella", "telephone", "sorry", "rhino", "quail"));
    SortUtil.mergesort(descstring, stringcomparator);
    SortUtil.mergesort(large, integercomparator);
    SortUtil.mergesort(average, integercomparator);
    SortUtil.mergesort(small, integercomparator);
    SortUtil.mergesort(one, integercomparator);
    assertTrue(isSorted(descstring,stringcomparator));
    assertTrue(isSorted(large, integercomparator));
    assertTrue(isSorted(average,integercomparator));
    assertTrue(isSorted(small,integercomparator));
    assertTrue(isSorted(one,integercomparator));
    
  }
  @Test
  public void testMergeSortEdgeCases() {
    ArrayList<Integer> intall = new ArrayList<Integer>(Arrays.asList(1,1,1,1,1,1,1,1,1,1));
    ArrayList<String> stringall = new ArrayList<String>(Arrays.asList("hi", "hi", "hi", "hi"));
    ArrayList<String> pairs = new ArrayList<String>(Arrays.asList("yes", "yes", "no", "no", "maybe", "maybe"));
    ArrayList<Integer> samesubarr = new ArrayList<Integer>(Arrays.asList(2,4,6,8,10,12,2,4,6,8,10,12));
    ArrayList<Integer> neg = new ArrayList<Integer>(Arrays.asList(6, 5, 7, 9, 12, 25, -2, 0, -10,-5, -12, -30));
    ArrayList<Double> empty = new ArrayList<Double>();
    SortUtil.mergesort(neg, integercomparator);
    SortUtil.mergesort(empty, doublecomparator);
    SortUtil.mergesort(pairs, stringcomparator);
    SortUtil.mergesort(samesubarr, integercomparator);
    SortUtil.mergesort(stringall, stringcomparator);
    SortUtil.mergesort(intall, integercomparator);
    assertTrue(isSorted(neg,integercomparator));
    assertTrue(isSorted(pairs,stringcomparator));
    assertTrue(isSorted(empty,doublecomparator));
    assertTrue(isSorted(stringall,stringcomparator));
    assertTrue(isSorted(intall, integercomparator));
    assertTrue(isSorted(samesubarr, integercomparator));

  }
  
  public static String generateRandomString(int length) {
    String characters = new String("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
    int n = characters.length();
    String random = new String();
    Random r = new Random();
    for (int i = 0; i < length; i++) {
      random += characters.charAt(r.nextInt(n));
    }
    return random;
  }
  
  public static <T> boolean isSorted(ArrayList<T> array, Comparator<T> comparator){    
    if (array.size() <= 1) {
      return true;
    }
    for (int i = 0; i < array.size()-1; i++) {
      if (comparator.compare(array.get(i), array.get(i+1))>0) {
        return false;
      }
    }
    return true;
  }

  
  /** Comparator methods for objects
  *
  */
  final class IntegerComparator implements Comparator<Integer> {

    @Override
    public int compare(Integer o1, Integer o2) {
      return o1.compareTo(o2);
    }
  }
  final class StringComparator implements Comparator<String> {

    @Override
    public int compare(String o1, String o2) {
      return o1.toString().compareToIgnoreCase(o2.toString());
    }
  }
  final class DoubleComparator implements Comparator<Double> {

    @Override
    public int compare(Double o1, Double o2) {
      return o1.compareTo(o2);
    }
  }
  final class CharacterComparator implements Comparator<Character> {

    @Override
    public int compare(Character o1, Character o2) {
      return o1.compareTo(o2);
    }
  }
}
