//
//  Shell.hpp
//  Assignment 2-Your Own Shell
//
//  Created by Irene Yeung on 2/1/18.
//  Copyright © 2018 Irene Yeung. All rights reserved.
//

#ifndef Shell_hpp
#define Shell_hpp

#include "shelpers.hpp"
#include <signal.h>

#include <sys/wait.h>
#include <stdio.h>
#include <string>
#include <cstdlib>
#include <iostream>


void initShell(std::string arg);

void executeCommand(Command command);

void changeDirectory(std::string dir);

void checkForZombies(int z);
#endif /* Shell_hpp */
