//
//  simulator.cpp
//  CS6015-DiscreteEventSim
//
//  Created by Irene Yeung on 2/16/18.
//  Copyright © 2018 Irene Yeung. All rights reserved.
//

#include "simulator.hpp"
#include <random>
#include <cassert>
#include <cmath>

int main(int argc, const char * argv[]) {
  if (argc != 2 ) {
    std::cerr << "Invalid user input: program accepts two command-line arguments.\n";
    exit(1);
  }
  std::string s = argv[1];
  if (!(s == "bank" || s == "supermarket")) {
    std::cerr << "Invalid command-line argument simulation scenario\n";
    exit(1);
  }

  setupSimulation(s);
  return 0;
}

void setupSimulation(std::string s){
  const int NUM_CUST = SIM_LENGTH/ARR_INT;
  SimulationType sim;
  if (s == "bank") {
    sim = BANK;
  }
  else if (s == "supermarket") {
    sim = SUPERMARKET;
  } else {
    std::cerr << "Error: Invalid Simulation Scenario Type\n";
    exit(1);
  }
  //Initialize data structures
  std::queue<Customer> waitqueue; //Store customers waiting in queue
  std::priority_queue<Event, std::vector<Event>, CompEvents> event_queue; //PQ storing events sorted ascending by event time
  std::queue<Employee> avail_tellers;//Track free tellers/cashiers
  std::vector<Employee> busy_tellers;//Track busy tellers/cashiers
  std::vector<Customer> customershelped; //For computing statistics
  
  //Add tellers/cashiers
  for (int i = 0; i < NUM_EMPL; i++) {
    Employee e(i);
    avail_tellers.push(e);
  }
  
  //Use uniform distribution int generator to generate random processing time for each customer
  std::random_device randseed;
  std::mt19937 gen(randseed());
  std::uniform_int_distribution<> t_transation(MIN_SERV,MAX_SERV);
  
  for (int i = 0; i < NUM_CUST; i++) {
    int transactiontime = t_transation(gen);//random int generator
    Customer cu(i, transactiontime); //Initialize Customer objects
    cu.T_arr = i * ARR_INT;
    Event arrevent(i * ARR_INT, CUST_ARR, cu);
    event_queue.push(arrevent); //Add arrival events to event_queue
  }
  
  //Initialize local data variables
  int total_idle_time = 0; //tracks total time at least 1 idle cashier
  int cumulative_wait = 0;
  int curr_time = 0; //Current time frame of simulatino
  int customers_helped = 0;
  int cust_wait_start = 0;
  int teller_idle_start = 0;
  int time_elapsed_since_last_event = 0;
  while (event_queue.size() > 0){
    if (curr_time >= SIM_LENGTH){
      break;
    }
    while (curr_time < SIM_LENGTH) {
      //Handle case where all customers/events processed before end of simulation time
      if (event_queue.size() == 0) {
        if (customers_helped == NUM_CUST){
          total_idle_time += (SIM_LENGTH - curr_time); //add remaining simulation time to total idle time
          assert(total_idle_time <= SIM_LENGTH);
          curr_time = SIM_LENGTH; //Advance time to end of simulation
          break;
        }
        //Handle case where gaps between departures and next arrivals
        else if (customers_helped < NUM_CUST){
          curr_time += ARR_INT;
          continue;
        }
      } else {
        time_elapsed_since_last_event = curr_time; //stores previous time frame
        assert(event_queue.size() > 0); //Prevent undefined behavior
        Event currevent = event_queue.top(); //next event of pq will be event with soonest event firing time
        event_queue.pop();
        curr_time = currevent.T_event; //Update current time to next frame of simulation
        time_elapsed_since_last_event = curr_time - time_elapsed_since_last_event;
        
        //PROCESS CUSTOMER ARRIVAL
        
        if (currevent.type == CUST_ARR) {
          if (avail_tellers.size() == 0){ //No available tellers
            assert(busy_tellers.size() == NUM_EMPL);
            if (sim == SUPERMARKET){
              cust_wait_start = curr_time;
              //Add them to shortest line of customers
              
              Employee& shortestline = findShortestLine(busy_tellers, curr_time);
              currevent.c.linenum = shortestline.ID;
              shortestline.customers->push_back(currevent.c);
            }
            else if (sim == BANK) {
              waitqueue.push(currevent.c); //Add them to the line
            }
          }
          else if (avail_tellers.size() > 0) { //A teller is available
            Employee& e = avail_tellers.front();
            currevent.c.linenum = e.ID;
            busy_tellers.push_back(e);
            avail_tellers.pop(); //remove front teller from avail tellers
            assert(busy_tellers.size() + avail_tellers.size() == NUM_EMPL);
            e.customers->push_back(currevent.c);
            if (avail_tellers.size() == 0){ //Update idle teller time
              total_idle_time += (curr_time - teller_idle_start);
            }
            
            //Now that customer is being helped, add their departure to queue
            int currdeparttime = curr_time + currevent.c.T_process;
            Event currdepart(currdeparttime, CUST_DEP, currevent.c);
            currdepart.empl = e.ID;
            event_queue.push(currdepart);
          }
        }
        //PROCESS CUSTOMER DEPARTURE
        else {
          assert(currevent.type == CUST_DEP);
          Employee freedempl = getFreedEmployee(busy_tellers, currevent.c.linenum);
          removeCustomerFromLine(freedempl, currevent.c); //Updates employee's lines
          customers_helped++;
          processCustomerCompletion(currevent.c, curr_time);
          assert(currevent.c.T_queue >= 0 && currevent.c.T_total > 0);
          customershelped.push_back(currevent.c);
          
          if ((sim == BANK && !waitqueue.empty())||(sim == SUPERMARKET && freedempl.customers->size() > 0)) { //customers are waiting, help next in line
            Customer nextinline;
            if (sim == BANK) {
              assert(waitqueue.size() > 0);
              nextinline = waitqueue.front();
              waitqueue.pop(); //help next customer; update line
              nextinline.linenum = freedempl.ID;
              freedempl.customers->push_back(nextinline);
            }
            else if (sim == SUPERMARKET){
              nextinline = freedempl.customers->front();
              nextinline.linenum = freedempl.ID;
            }
            cumulative_wait += (curr_time - cust_wait_start);
            //Add departure of new customer being helped to event queue
            int nextdeparttime = curr_time + nextinline.T_process;
            Event nextdepart(nextdeparttime, CUST_DEP, nextinline);
            nextdepart.empl = freedempl.ID; //set employee to employee now helping them
            event_queue.push(nextdepart);
          }
          else if ((sim == BANK && waitqueue.empty())||(sim == SUPERMARKET && freedempl.customers->size() == 0)){  //add teller back to available tellers list (and remove from busy tellers list
            int emplIDtofree = freedempl.ID;
            assert(busy_tellers.size() > 0);
            int index = -1;
            for (int i = 0; i < busy_tellers.size(); i ++){
              if (busy_tellers.at(i).ID == emplIDtofree){
                index = i; //Find index of freedempl in busy_tellers list
                busy_tellers.erase(busy_tellers.begin() + index);
                break;
              }
            }
            avail_tellers.push(freedempl);
            assert(avail_tellers.size() + busy_tellers.size() == NUM_EMPL);
            teller_idle_start = curr_time; //start of next idle period
          }
        }
      }
    }
  }
  printSimulationData(customershelped, cumulative_wait, total_idle_time);
  
  //Delete dynamically allocated memory 
  //  while (avail_tellers.size() > 0) {
  //    Employee& e = avail_tellers.front();
  ////    if (avail_tellers.front().customers
  ////        != NULL){
  //    delete e.customers;
  ////    }
  //    avail_tellers.pop();
  //  }
  //  for (int i = 0 ; i < busy_tellers.size(); i++){
  ////    if (busy_tellers.front().customers
  ////        != NULL){
  //    Employee& e = busy_tellers.front();
  //    delete e.customers;
  //    busy_tellers.pop_back();
  //
  ////    }
  //  }
  ////  assert (avail_tellers.size() + busy_tellers.size() == 0);
}

void processCustomerCompletion(Customer& c, int depart){//once a customer has finished, update their wait and service time
  c.T_queue = depart - (c.T_arr + c.T_process);
  c.T_total = c.T_queue + c.T_process;
}

void printSimulationData(std::vector<Customer> customershelped, int cumulative_wait, int total_idle_time){
  const int NUM_CUST = SIM_LENGTH/ARR_INT;
  
  int avgserv = 0;
  double servtimevar = 0;
  if (DEBUG){
    std::cout << "\n\n\n\n\n\nSimulation Statistics:\n\n\n";
    std::cout << "Number of Tellers:\t" << NUM_EMPL << std::endl;
    std::cout << "Number of Customers:\t" << NUM_CUST << std::endl;
  }
  size_t numhelped = customershelped.size();
  int totalqueuetime = 0;
  for (int i = 0; i < numhelped; i++) {
    Customer c = customershelped[i];
    if (DEBUG) {
      std::cout << "Customer #" <<c.ID << std::endl;
      std::cout << "Processing time: " << c.T_process << std::endl;
      std::cout << "Arrival Time: " << c.T_arr << "\nWaiting time: " << c.T_queue << "\n\nTotal Service Time: " << c.T_total << " seconds. \n\n";
    }
    avgserv += c.T_total;
    totalqueuetime += c.T_queue;
  }
  avgserv /= numhelped;
  //Calculate variance in service times
  for (int i = 0; i < numhelped; i++) {
    servtimevar += (customershelped[i].T_total - avgserv)*(customershelped[i].T_total - avgserv);
  }
  servtimevar /= numhelped;
  
  std::cout << "Total Simulation Length:\t" <<  SIM_LENGTH << " seconds.\n";
  std::cout << "Average Customer Service Time:\t" << avgserv*1.0 << " seconds.\n";
  std::cout <<"Variance (Standard Deviation) in Customer Service Time:\t" << std::sqrt(servtimevar) << std::endl;
  std::cout <<"Total Number of Customers Served:\t" << numhelped << std::endl;
  std::cout << "Percentage of simulation time that there was at least 1 idle cashier:\t" << (100.0 * total_idle_time)/SIM_LENGTH << "%.\n\n";
  
  if (DEBUG){
    std::cout <<"\nTotal idle time: " << total_idle_time*1.0 << std::endl;
    std::cout << "\nPercentage of simulation time that there was at least 1 idle cashier: " << (100.0 * total_idle_time)/SIM_LENGTH << "%\n";
    std::cout << "\nCumulative wait time of customers: using incremented throughout while loop " << cumulative_wait << std::endl;
    std::cout << "\n\nSum of all the T_queues for each Customer object: (SHOULE B EQUAL TO CUM WAIT TIME --- using sum of all struct field T_queue)" << totalqueuetime << std::endl;
    std::cout << "\n\nAverage customer wait time: " << (cumulative_wait*1.0)/numhelped << std::endl;
    std::cout << "\nAverage queue time of all customers that were able to complete their transaction during the simulation period: (SHOULD BE EQUAL" << totalqueuetime/numhelped *1.0 << std::endl;
  }
}

int getTotalWait(Employee cashier, int currtime){
  if (cashier.customers->size() == 0){ //No customers in line
    return 0;
  }
  std::vector<Customer> *line = cashier.customers;
  Customer cust = line->front();
  int currcustremaining= (cust.T_arr + cust.T_total) - currtime;
  int wait = currcustremaining; //remaining service time of customer being processed
  if (line->size() == 1){
    return currcustremaining;
  }
  size_t len = line->size();
  for (int i = 1; i < len; i++){
    Customer c = line->at(i);
    wait += c.T_process;
  }
  return wait;
}


Employee& findShortestLine(std::vector<Employee> lineswithcustomers, int currtime){
  assert(lineswithcustomers.size() > 0);
  Employee& fastest = lineswithcustomers.front();
  if (lineswithcustomers.size() == 1){
    return fastest;
  }
  int minwait = getTotalWait(fastest, currtime);
  for (int j = 1; j < lineswithcustomers.size(); j++) {
    int wait = getTotalWait(lineswithcustomers.at(j),currtime);
    if (wait < minwait) {
      minwait = wait;
      fastest = lineswithcustomers.at(j);
    }
  }
  return fastest;
}

Employee getFreedEmployee(std::vector<Employee> busy_tellers, int IDnum){
  assert(busy_tellers.size() > 0);
  Employee freedempl = busy_tellers[0];
  if (busy_tellers.size() == 1){
    return busy_tellers[0];
  }
  for (int i = 0; i < busy_tellers.size(); i++) {
    if (busy_tellers.at(i).ID == IDnum){
      freedempl = busy_tellers.at(i);
    }
  }
  return freedempl;
}

void removeCustomerFromLine(Employee& e, Customer c){
  size_t linelen = e.customers->size();
  assert (linelen > 0);
  for (int i = 0; i < linelen; i++) {
    if (e.customers->at(i).ID == c.ID){
      e.customers->erase(e.customers->begin() + i);
      assert(e.customers->size() == linelen -1);
      return;
    }
  }
}



Customer::Customer(){
}

Employee::Employee(){
}

Customer::~Customer(){
}

Employee::~Employee(){
}

Event::~Event(){
}

