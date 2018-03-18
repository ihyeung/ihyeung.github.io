package assignment04;

import java.io.FileNotFoundException;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.HashSet;
import java.util.Random;

public class SortUtil<T>{
  private static int threshold;
  public static void main(String[] args) {
    mergeVsQuick(1000);
  }
  public static<T> void mergeVsQuick(int n){
    IntegerComparator comp = new IntegerComparator();

    while (n <= 20000) {

      long startTime, midpointTime, stopTime, arrayCopyStart, arrayCopyEnd;

      //Thread stabilization
      startTime = System.nanoTime();
      while (System.nanoTime() - startTime < 1000000000) {
      }

      //           
      long timesToLoop = 100000;
      ArrayList<Integer> avg = generateAverageCase(n); 
      //      ArrayList<Integer> worst = generateWorstCase(n);
      //    ArrayList<Integer> best = generateBestCase(n);

      // Run test
      startTime = System.nanoTime();

      arrayCopyStart = System.nanoTime();
      //    
      ArrayList<Integer> temp = new ArrayList<Integer>(avg.size());
      for (int i = 0; i < avg.size(); i++){
        temp.add(i,avg.get(i));
      }
      arrayCopyEnd = System.nanoTime();

      for (long i = 0; i < timesToLoop; i++) {
        quicksort(temp,comp);
        //mergesort(temp,comp);
        //quicksort(worst,comp);
        //mergesort(worst,comp);
        //quicksort(best,comp);
        //mergesort(best,comp);

      }

      midpointTime = System.nanoTime();

      // Empty loop for adjusting for loop iteration cost

      for (long i = 0; i < timesToLoop; i++) { // empty block
      }

      stopTime = System.nanoTime();

      double averageTime = ((midpointTime - startTime) - (stopTime - midpointTime) -(arrayCopyEnd - arrayCopyStart)) / timesToLoop; // for generateAverageCase()
      //      double averageTime = ((midpointTime - startTime) - (stopTime - midpointTime)) / timesToLoop; // for generateBestCase() and generateWorstCase()

      System.out.println(averageTime); //runtime in nanoseconds
      n+=1000;
    }

  }

  /**
   * Mergesort driver method.
   * Performs a mergesort on the generic ArrayList given as input.
   *@param arr input arraylist
   *@param comparator 
   */
  public static <T> void mergesort(ArrayList<T> arr, Comparator<? super T> comparator) {
    if (arr.size()>1 && !isSorted(arr,(Comparator<T>)comparator)) {
      ArrayList<T> temp =  new ArrayList<T> ();
      for (int i = 0 ; i < arr.size(); i++) {  //initialize elements to null to use arr.set() below
        temp.add(i, null);
      }
      mergeSort(arr,temp, 0, arr.size(), comparator);
    }
  }
  /**
   * Mergesort recursive method.
   * If array length is less than threshold value, sorts using
   * insertion sort.
   *  
   * @param arr  subarray to perform mergesort on
   * @param start  beginning index of subarray
   * @param end index of final element of subarray
   */
  private static <T> void mergeSort(ArrayList<T> arr, ArrayList<T> temp, int start, int end, Comparator<? super T> comparator) {
    if (start < end) {
      int mid = (start + end)/ 2;
      threshold = 25 ; // Optimal insertion sort threshold value determined from part 1 
      if (end - start <= threshold) { //implement insertion sort for smaller subarrays
        insertionSort(arr, comparator, start, end);
      } else {
        mergeSort(arr, temp, start, mid, comparator);
        mergeSort(arr, temp, mid+1, end, comparator);
      }
      merge(arr,temp, start, mid + 1, end, comparator);
    }

  }
  /**
   * Merges sorted subarrays.
   * 
   * @param arr array to sort
   * @param temp temp storage array
   * @param leftPoint index of start of left subarray
   * @param rightPoint index of start of right subarray
   * @param rightEnd index of end of right subarray 
   * @param comparator comparator passed in to use compare() method
   */
  private static <T> void merge(ArrayList<T> arr, ArrayList<T> temp,int leftPoint, int rightPoint, int rightEnd, Comparator<? super T> comparator) {
    int leftEnd = rightPoint-1;
    int tempPos = leftPoint;
    int size = rightEnd - leftPoint + 1;
    while(leftPoint <= leftEnd && rightPoint <= rightEnd) { //first while loop when both pointers are traversing arrays
      if (comparator.compare(arr.get(leftPoint), arr.get(rightPoint)) <= 0){ //compare values at pointers from left and right arrays
        temp.set(tempPos++, arr.get(leftPoint++)); //add leftPoint element to temp 
      } else {     
        temp.set(tempPos++, arr.get(rightPoint++));//else rightPoint element is added to temp array
      }
    }
    //Note: only one of the two below while loops will execute!
    while (leftPoint <= leftEnd) { //while loop for if rightPoint reaches end of array first
      temp.set(tempPos++, arr.get(leftPoint++));
    }
    while (rightPoint <= rightEnd) {//while loop for if leftPoint reaches end of array first
      temp.set(tempPos++, arr.get(rightPoint++));
    }
    for (int i = 0; i < size; i++, rightEnd--) { //copy values from temp back into array
      arr.set(rightEnd, temp.get(rightEnd));
    }
  }
  /**
   * QuickSort Driver method
   * Performs a quicksort on the generic ArrayList given as input.
   * @param array
   * @param comparator
   */
  public static <T> void quicksort(ArrayList<T> array, Comparator<? super T> comparator) {
    if (array.size() > 1 && !isSorted(array,(Comparator<T>) comparator)) {
      quickSort(array, comparator, 0, array.size()-1);
    }
  }
  /**
   * Recursive QuickSort method
   */
  public static <T> void quickSort(ArrayList<T> array, Comparator<? super T> comparator, int start, int end) {
    if (start<end) {
      threshold = 25;
      if (end-start <= threshold) {
        insertionSort(array, comparator, start, end);
      } else {
        int pivot = partition(array,start, end, comparator, medianOfThree(array,start, end, comparator)); 
        quickSort(array,comparator,start,pivot-1);
        quickSort(array, comparator, pivot+1, end);
      }
    }
  }
  private static<T> int partition(ArrayList<T> array, int low, int high, Comparator<? super T> comparator, int pivotindex) {

    // Pivot Selection Strategies (only execute one line, comment out other lines):
    //int pivotindex = medianOfThree(array,low, high, comparator);
    //int pivotindex = high;
    // int pivotindex= middlePivot(array, low, high, comparator);
    //int pivotindex= randomPivot(array, low, high, comparator);

    swap(array,pivotindex,high); //moves pivot to last element of array
    T pivot = array.get(high);
    int left = low;
    int right = high-1;//right pointer starts at high-1 since index high contains pivot

    while(left <= right){ //when L pointer meets or passes R pointer, break out of while loop
      if (comparator.compare(array.get(left), pivot) <0) {//left elements should be smaller than pivot, right elements larger
        left++;//do nothing, shift pointer right if LHS element smaller than pivot
        continue;
      }
      if (comparator.compare(array.get(right),pivot) > 0 && right > 0) {//left elements should be smaller than pivot, right elements larger
        right--; //do nothing, shift pointer left if RHS element larger than pivot
        continue;
      }
      //if neither of above if loops are executed, means elements need to be swapped
      swap(array,left,right); //ie array[left] > pivot or array[right] < pivot, swap array[left] and array[right]
      left++;
      right--;
    }
    swap(array,left,high); //after while loop breaks, pivot belongs where L pointer is, swap arr[L] and arr[pivot]
    return left; //arr[L] contains pivot element, return pivot index, ie left
  }

  /**
   * Finds median of 3 elements to use as pivot
   * @param array
   * @param high
   * @param low
   * @param comparator
   * @return
   */
  private static<T> int medianOfThree(ArrayList<T> array, int high, int low, Comparator<? super T> comparator) {
    int mid = (low + high)/2;
    if (comparator.compare(array.get(mid),array.get(low))<0) {
      swap(array,mid,low);
    }
    if (comparator.compare(array.get(high), array.get(low))<0) {
      swap(array,low,high);
    }
    if (comparator.compare(array.get(high), array.get(mid))<0) {
      swap(array,high,mid); 
    }
    return mid;
  }

  private static<T> int middlePivot(ArrayList<T> array, int high, int low, Comparator<? super T> comparator) {
    int mid = (low + high)/2;
    return mid;
  }

  private static<T> int randomPivot(ArrayList<T> array, int low, int high, Comparator<? super T> comparator) {
    Random r = new Random();
    //Returns pivot within the range (low, high)
    return r.nextInt(high-low + 1) + low;
  }
  /**
   * Swap ArrayList elements
   * @param array
   * @param i
   * @param j
   */
  private static <T> void swap(ArrayList<T> array, int i, int j){
    T temp = array.get(i);
    array.set(i, array.get(j));
    array.set(j, temp);
  }
  /**
   * InsertionSort method
   * Performs insertionsort on portion of generic ArrayList between defined indices
   * @param arr generic ArrayList
   * @param comparator
   * @param left
   * @param right
   */
  private static <T> void insertionSort(ArrayList<T> arr, Comparator<? super T> comparator, int left, int right) {
    for (int i = left; i <= right; i++) {
      T currentElement = arr.get(i);
      int currentIndex = i;
      for (; currentIndex > left && comparator.compare(currentElement, arr.get(currentIndex-1)) < 0; currentIndex--) {
        arr.set(currentIndex, arr.get(currentIndex-1));
      }
      arr.set(currentIndex, currentElement);
    }
  }
  /**
   * Random AlphaNumeric String Generator
   * Used for testing
   * @param length
   * @return random string 
   */
  public static String generateRandomString(int length) {
    String characters = new String("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
    int n = characters.length();
    String random = new String();
    Random r = new Random(5);
    for (int i = 0; i < length; i++) {
      random += characters.charAt(r.nextInt(n));
    }
    return random;
  }
  /**
   * Checks if a list is sorted.
   * @param array
   * @param comparator
   * @return boolean
   */
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
  /**
   * Generates a best case ArrayList<Integer> for testing.
   * @param size array size
   * @return ArrayList<Integer> with ascending elements from 1 to size
   */
  public static ArrayList<Integer> generateBestCase(int size){
    ArrayList<Integer> ints = new ArrayList<Integer>();
    for (int i = 1; i <= size; i++) {
      ints.add(i);
    }
    return ints;
  }
  /**
   * Generates an average case ArrayList<Integer> for testing.
   * 
   * @param size array size
   * @return ArrayList<Integers> with permuted elements 
   */
  public static ArrayList<Integer> generateAverageCase(int size){
    ArrayList<Integer> ints = generateBestCase(size);
    permuteIntegerArray(ints);
    return ints;
  }
  /**
   * Shuffles elements of an Integer ArrayList using a
   * seed to a Java Random object.
   * @param ArrayList<Integer> array to permute
   */
  public static void permuteIntegerArray(ArrayList<Integer> arr) {
    Random r = new Random(17);
    for (int i = 0; i < arr.size(); i++) {
      int temp = arr.get(i);
      int j = r.nextInt(arr.size());
      arr.set(i, arr.get(j));
      arr.set(j, temp);
    }
  }

  /**
   * Generates a worst case ArrayList<Integer> for testing.
   * @param size
   * @return ArrayList<Integer> with elements 1 through size in descending order.
   */
  public static ArrayList<Integer> generateWorstCase(int size){
    ArrayList<Integer> ints = new ArrayList<Integer>();
    for (int i = size; i > 0; i--) {
      ints.add(i);
    }
    return ints;
  }

}
/**
 * 
 * Comparator functors
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




