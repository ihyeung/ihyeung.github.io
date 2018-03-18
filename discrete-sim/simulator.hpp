//
//  simulator.hpp
//  CS6015-DiscreteEventSim
//
//  Created by Irene Yeung on 2/16/18.
//  Copyright © 2018 Irene Yeung. All rights reserved.
//

#ifndef simulator_hpp
#define simulator_hpp
#define ARR_INT 32 //Interval in seconds between customer arrivals
//#define SIM_LENGTH 28800 //Total simulation length (8 hours)
#define SIM_LENGTH 1000000
//Changed total simulation length to 1000 * 1000 * 100 to stabilize
#define NUM_EMPL 10
#define MIN_SERV 30
#define MAX_SERV 600

#define DEBUG 0
#define debug_print(args ...) if (DEBUG) fprintf(stderr, args)

#include <stdio.h>
#include <cstdlib>
#include <string>
#include <queue>
#include <iostream>
#include <algorithm>

enum EventType {CUST_ARR = 0, CUST_DEP};
enum SimulationType {BANK = 1, SUPERMARKET};

struct Employee;

struct Customer {
  int ID;
  int T_arr; //Time of arrival
  int T_process; //Transaction/processing time
  int T_queue = {}; //Time spent waiting in queue
  int T_total = {}; //T_queue + T_process
  int linenum = -1; //shopping line customer is in, ie employee num
  Customer(int ID, int T_process)
    : ID(ID), T_arr(ID * ARR_INT), T_process(T_process),
    T_queue(0), T_total(0) {} //Initialize linenum to 0, initilaize T_total and T_queue to -1 for error checking
  Customer();
  ~Customer();
};

struct Employee {
  int ID; 
  std::vector<Customer> * customers;
  Employee(int ID) : ID(ID), customers(new std::vector<Customer> ()) {}
  Employee();
  ~Employee();
};

struct Event {
  int T_event;//event firing time; parameter to sort priority queue with later
  EventType type;
  Customer c;
  int empl; //Track which employee is finished if CUST_DEP event, otherwise -1
  Event(int time, EventType type, Customer c) : T_event(time), type(type), c(c), empl(-1) {}
  ~Event();
};

struct CompEvents { //Custom comparator struct for event ordering
  bool operator()(Event e1, Event e2) {
    return e1.T_event > e2.T_event; //Orders ascending event times
  }
};

bool sortTellers(Employee e1, Employee e2) { 
  return e1.customers->size() < e2.customers->size();
};

Employee getFreedEmployee(std::vector<Employee> busy_tellers, int IDnum);
void removeCustomerFromLine(Employee& e, Customer c);
Employee& findShortestLine(std::vector<Employee> lineswithcustomers, int currtime);
int getTotalWait(Employee cashier, int currtime);
void setupSimulation(std::string s);
void processEvent(Event& event, int curr_time);
void processCustomerCompletion(Customer& c, int depart);
void printSimulationData(std::vector<Customer> customershelped, int wait, int idle);

#endif /* simulator_hpp */
