//
//  main.cpp
//  Assignment 2-Your Own Shell
//
//  Created by Irene Yeung on 2/1/18.
//  Copyright © 2018 Irene Yeung. All rights reserved.
//

#include <iostream>
#include "shelpers.hpp"
#include "Shell.hpp"
#include <cassert>
#include <string>

int main(int argc, const char * argv[]) {
  std::string line = *argv;
  initShell(line);
  std::cout << "Hello, World!\n";
  return EXIT_SUCCESS;
}
