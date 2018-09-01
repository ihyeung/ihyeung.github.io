//
//  Shell.cpp
//  Assignment 2-Your Own Shell
//
//  Created by Irene Yeung on 2/1/18.
//  Copyright © 2018 Irene Yeung. All rights reserved.
//
#include <iostream>
#include "Shell.hpp"
#include <string>
#include <vector>
#include <sys/types.h>
#include <unistd.h>

void initShell(std::string arg){
  std::vector<std::string> c;
  std::string quit = "exit";
  std::vector<Command> command;
  while (getline(std::cin, arg)) {
    if (arg.find(quit) != std::string::npos){ //npos means end of string was reached
      std::cout << "Exiting shell . . . \n";
      exit(EXIT_SUCCESS);
    }
    if (arg.size() == 0){
      break;
    }
    c = tokenize(arg);
    std::vector<Command> command = getCommands(c);
    for (int i = 0; i < command.size(); i++) {
      std::cout << "Executing the command " << command[i].exec << std::endl;
      executeCommand(command.at(i));
      if (command.at(i).background){
        signal(SIGCHLD, checkForZombies);
      }
    }
    
  }
}

void executeCommand(Command command){
  
  //Handle cd builtin -- if cd, use chdir and do not fork/exec!
  if (command.exec == "cd"){
    std::string dirname;
    const int DIR_NAME = 1;
    
    if (command.argv.size() == 2){ //ie cd to root
      changeDirectory(dirname);
    }
    else if (command.argv.size() == 3){ //Directory name with no spaces
      dirname = command.argv.at(DIR_NAME);
    }
    else if (command.argv.size() > 3) { //Directory name has spaces, add them back
      for (int i = 1 ; i < command.argv.size() -1 ; i++) {
        dirname += command.argv.at(i);
        dirname += " ";
      }
      dirname = dirname.substr(0, dirname.size()-1); //Truncate last space added
      std::cout << "Directory to change to : \"" << dirname << "\""<< std::endl;
    }
    changeDirectory(dirname);
  }
  else {
  const char * pExec = const_cast<char *>(command.exec.c_str()); //.c_str() includes null terminator
  char ** pArr = const_cast<char **>(command.argv.data()); //pointer to array of char strings containing list of arguments given from command line
  int status;
  pid_t PID; //Creates a copy of shell process
  
  if ((PID = fork()) == 0){ //Child process, call dup2 then execvp here
      if (dup2(command.fdStdin, 0) < 0){
        perror("DUP 2 of fstdin to stdin fd failure\n");
        exit(EXIT_FAILURE); //Terminate if syscall error
      }
      if (dup2(command.fdStdout,1) < 0) {
        perror("DUP 2 of fstdout to stdout fd failure\n");
        exit(EXIT_FAILURE); //Terminate if syscall error
      }
    //before you exec, make sure to close pipe (go through commands and check for any stdin not 0 and any stdout not 1, close any that are not the stdin = 0 or stdout = 1
   if (execvp(pExec, pArr) == -1) { //After dup2, now call execvp
      perror("Execvp error\n");
      exit(EXIT_FAILURE);
    }
  }
  else if (PID < 0){
      perror("Fork error\n");
      exit(EXIT_FAILURE);
  } else { //Parent process
    if (!command.background) {
      if (waitpid(PID, &status, 0) < 0) { //Wait for child PID to terminate, returns PID of child process after it terminates
//        std::cout << "Signal delivered to calling process for pid " << PID << "\n";
      }
    }
    //fd cleanup: close any extraneous file descriptors that were created (ie not 0-2)
    std::cout << "Child proess completed\n";
      if (command.fdStdin != 0) {
        close(command.fdStdin);
      }
      if (command.fdStdout != 1) {
        close(command.fdStdout);
      }
  }
  
  }
  std::cout << "Command execution complete\n";
}

//Each process has its own working directory, cd only modifies calling process,
//not parent process. hence needs to be a builtin
void changeDirectory(std::string dir){
  const std::string HOME = getenv("HOME");
  std::string newdir; //Destination directory
  
  //Note: If newly created child process, OLDPWD is cleared by bash
  //If new child process, initialize value of OLDPWD to PWD ..
  
  setenv("OLDPWD", getenv("PWD"),1); //First move PWD to OLDPWD
  if (dir == "" || dir == "~"){ //If cd to root directory
    newdir = getenv("HOME");
  }
  else if (dir == ".."){ //Navigate up one directory
    std::string d = getenv("PWD");
    size_t slashes = std::count(d.begin(), d.end(),'/');
    size_t index = d.find_last_of("//");
    if (slashes < 2){ //If no more parent directories, go to root
      newdir = getenv("HOME"); //Resets filepath to HOME
    } else {
    newdir = d.substr(0, index); //Extracts filepath one directory up
    }
  }
  else {
    std::string basedir = getenv("PWD"); //Base current working directory
    newdir = basedir + "/" + dir; //Compose absolute filepath
  }
  setenv("PWD", newdir.c_str(), 1); //Update PWD variable to new filepath
  if (chdir(newdir.c_str()) < 0){ //chdir works with relative or absolute filepath
    perror("chdir() error\n");
    exit(EXIT_FAILURE); //Exit if syscall error
  }
}

void checkForZombies(int z){
  int status;
  pid_t PID;
  if ((PID= waitpid(0, &status, WNOHANG)) == 0) {
    std::cout << "None of children specified by pid have changed state\n";
  }
  else if (PID == -1){
    perror("wait pid value is -1\n");
  } else {
    std::cout << "PID of child process with status change: " << PID << std::endl;
  }
  
}
