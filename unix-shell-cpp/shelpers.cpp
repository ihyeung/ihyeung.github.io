#include "shelpers.hpp"
#include <string>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>

/*
  text handling functions
 */

bool splitOnSymbol(std::vector<std::string>& words, int i, char c){
  if(words[i].size() < 2){ return false; }
  int pos;
  if((pos = words[i].find(c)) != std::string::npos){ //If string contains char c
	if(pos == 0){
	  //starts with symbol
	  words.insert(words.begin() + i + 1, words[i].substr(1, words[i].size() -1));
	  words[i] = words[i].substr(0,1);
	} else {
	  //symbol in middle or end
	  words.insert(words.begin() + i + 1, std::string{c});
	  std::string after = words[i].substr(pos + 1, words[i].size() - pos - 1);
	  if(!after.empty()){
		words.insert(words.begin() + i + 2, after);
	  }
	  words[i] = words[i].substr(0, pos);
	}
	return true;
  } else {
	return false;
  }

}

std::vector<std::string> tokenize(const std::string& s){

  std::vector<std::string> ret;
  int pos = 0;
  int space;
  //split on spaces
  while((space = s.find(' ', pos)) != std::string::npos){
	std::string word = s.substr(pos, space - pos);
	if(!word.empty()){
	  ret.push_back(word);
	}
	pos = space + 1;
  }

  std::string lastWord = s.substr(pos, s.size() - pos);
  if(!lastWord.empty()){
	ret.push_back(lastWord);
  }

  for(int i = 0; i < ret.size(); ++i){
	for(auto c : {'&', '<', '>', '|'}){
	  if(splitOnSymbol(ret, i, c)){
		--i;
		break;
	  }
	}
  }
  
  return ret;
  
}

std::ostream& operator<<(std::ostream& outs, const Command& c){
  outs << c.exec << " argv: ";
  for(const auto& arg : c.argv){ if(arg) {outs << arg << ' ';}}
  outs << "fds: " << c.fdStdin << ' ' << c.fdStdout << ' ' << (c.background ? "background" : "");
  return outs;
}

/* 
 Tokenizes command line input and generates a vector of Command structs.
 */
std::vector<Command> getCommands(const std::vector<std::string>& tokens){
  std::vector<Command> ret(std::count(tokens.begin(), tokens.end(), "|") + 1);  //1 + num |'s commands

  int first = 0;
  int last = std::find(tokens.begin(), tokens.end(), "|") - tokens.begin();
  bool error = false;
  for(int i = 0; i < ret.size(); ++i){
	if((tokens[first] == "&") || (tokens[first] == "<") ||
		(tokens[first] == ">") || (tokens[first] == "|")){
	  error = true;
	  break;
	}

	ret[i].exec = tokens[first];
	ret[i].argv.push_back(tokens[first].c_str()); //argv0 = command name
	ret[i].fdStdin = 0;
	ret[i].fdStdout = 1;
	ret[i].background = false;
	for(int j = first + 1; j < last; ++j){
	  if(tokens[j] == ">" || tokens[j] == "<" ){
      if (tokens[j] == ">"){
        if (i != ret.size() -1){ //Only last command can output redirect
          perror("Error: only last command can have output file redirection\n");
          error = true;
          break;
        }
        int fd = open(tokens[j+1].c_str(), O_WRONLY | O_CREAT, S_IRWXU);
        //Opens file in write-only mode, or creates new file with w/r/e permissions
        //if file does not exist.
        if (fd < 0){
          std:: cout << "opening file: " << tokens[j+1] << std::endl;
          perror("Output file redirection - File open error\n");
          exit(EXIT_FAILURE);
        } else {
            ret[i].fdStdout = fd; //Set command's fd out to redirection output file output fd
            j++;
        }
      }
      else if (tokens[j] == "<"){
        if (i != 0){ //Only first command can input redirect
          perror("Error: only first command can have input file redirection\n");
          error = true;
          break;
        }
        int fd = open(tokens[j+1].c_str(), O_RDWR);
        if (fd < 0){
          perror("Input file redirection file open failure; File does not exist\n");
          exit(EXIT_FAILURE);
        } else {
          j++;
          ret[i].fdStdin = fd; //Set stdin to be redirection input file's in fd
        }
      }
	  }
    else if(tokens[j] == "&"){
      ret[i].background = true;
	  } else {
		//otherwise this is a normal command line argument!
		ret[i].argv.push_back(tokens[j].c_str());
	  }
	}
	if(i > 0){ //ie multiple commands to be chained
    //Call pipe for each command assuming multiple commands
    int READ_END = 0;
    int WRITE_END = 1;
    int fds[2];
    pipe(fds); //Generates two new fds that arent already being used
  
    ret[i-1].fdStdout = fds[WRITE_END]; //set previous command's fd out to first fd in fds
    ret[i].fdStdin = fds[READ_END]; //set current command's fd in to second fd in fds
	 	}
	ret[i].argv.push_back(nullptr); //Null terminated char required by exec

	//find the next pipe character
	first = last + 1;
	if(first < tokens.size()){
	  last = std::find(tokens.begin() + first, tokens.end(), "|") - tokens.begin();
	}
 
  if(error){ //fd cleanup for early termination of function if error found
      for (Command c : ret){
        if (c.fdStdin != 0) {
          close(c.fdStdin);
        }
        if (c.fdStdout != 1) {
          close(c.fdStdout);
        }
      }
    }
  }
  return ret;
}
