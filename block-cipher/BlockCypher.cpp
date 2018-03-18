//
//  BlockCypher.cpp
//  Assignment1-BlockCypher
//
//  Created by Irene Yeung on 1/14/18.
//  Copyright © 2018 Irene Yeung. All rights reserved.
//

#include "BlockCypher.hpp"
#include <cstdlib>
#include <string>
#include <cmath>
#include <cassert>
#include <iostream>
#include <array>

Block computeKey(std::string pass){
  Block keyarr = {{0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0}};
  for (int i = 0; i < pass.size(); i++) {
    keyarr[i % 8] ^= pass[i];
  }
  return keyarr;
}

substitutionTable identityPermutation() {
  substitutionTable identity;
  uint8_t j = 0x0;
  for (int i= 0 ; i <= 255; i++,j++) {
    identity[i] = j;
  }
  return identity;
}

substitutionTable fisherYatesShuffle(substitutionTable table){
  uint8_t randind;
  uint8_t remainingswaps = table.size() - 1;
  while (remainingswaps > 0){
    randind  = rand() % (remainingswaps + 1);
    uint8_t temp = table[randind];
    table[randind] = table[remainingswaps];
    table[remainingswaps] = temp;
    remainingswaps--;
  }
  return table;
}

substitutionTableArr forwardSubstitutionTables(substitutionTable table){
  substitutionTableArr tablearr;
  substitutionTable prev = table;
  for (int i = 0; i < tablearr.size(); i++){
    tablearr[i] = fisherYatesShuffle(prev); //to get first substitution table, shuffle identity permutation
    prev = tablearr[i]; //each subsequent table is obtained by shuffling previous table
  }
  return tablearr;
}

substitutionTableArr inverseSubstitutionTables(substitutionTableArr forward){
  substitutionTableArr tablearr;
  for (int i = 0; i < 8; i++){
    substitutionTable table;
    for (int j = 0; j < table.size(); j++ ){
      uint8_t inv = forward[i][j]; //value of jth index element of the corresponding forward substitution table i
      table[inv] = j; //use inv value to access the index (ie j) in inverse table
    }
    tablearr[i] = table; //after generating values for all indices in forward table, add to array of tables
  }
  return tablearr;
}

void printBits(Block byteblock){ //Prints bits of a block of bytes with little-endian byte order
  for (int i = 0; i < byteblock.size(); i++) {
    std::bitset<8> bits;
    bits = byteblock[i];
    std::cout << bits << " ";
  }
}

uint8_t performByteSubstitution(uint8_t byte, substitutionTable subtable){
  return subtable[byte];
}

Block encryptBlock(Block block, Block key, substitutionTableArr subtable){
  const int NUM_ROUNDS = 16;
  for (int i = 0; i < NUM_ROUNDS; i++){
    for (int i = 0 ; i < block.size(); i++){
      block[i] ^= key[i]; //1. XOR bytes of block with corresponding bytes of key
    }
    for (int i = 0 ; i < block.size(); i++){
      block[i] = performByteSubstitution(block[i], subtable[i]); //2. Substitute bytes of block with corresponding tables
    }
    block = leftTransposeState(block, 1); //3. Bitwise rotate entire block left by 1 bits.
  }
  return block;
}

std::vector<Block> translateMessage(std::string plaintext){
  std::vector<Block> message;
  Block currdata;
  for (int i = 0; i < plaintext.size();i++){
    if (i % 8 == 0){ //Create new block of data when any existing block is filled
      Block newblock = {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};
      currdata = newblock;
    }
    currdata[i % 8] = plaintext[i];
    
    if (i % 8 == 7){ //Current block filled, add to message vector
      message.push_back(currdata);
    }
  }
  if (!(plaintext.size() % 8 == 0)){ //If message length is not a multiple of 8, last block will be partially filled, add last block to message vector
    message.push_back(currdata);
  }
  return message;
}

std::string readMessage (std::vector<Block> message, uint8_t length){ //Convert message to readable string message
  std::string stringmsg ;
  uint8_t end = 0;
  for (int i = 0; i < message.size(); i++){
    for (int j = 0; j < message[i].size(); j++, end++){
      if (end < length){
        stringmsg += (char)(message[i][j]);
      }
    }
  }
  // a message of length not divisible by 8 will have an partially filled final block
  assert(stringmsg.size() == length); //make sure empty bytes of final block (i.e., with value of 0x0) are not added to end of message string
  return stringmsg;
}

std::vector<Block> encryptMessage(std::string plaintext, Block key,
                                  substitutionTableArr forward){
  std::vector<Block> message = translateMessage(plaintext); //convert plaintext string to vector of Blocks to encrypt
  for (int i = 0; i < message.size(); i++){ //encrypt message one block at a time
    message[i] = encryptBlock(message[i], key, forward);
  }
  return message;
}

// Bit rotation (credit: https://blog.regehr.org/archives/1063)
uint8_t rotateLeft(uint8_t byte, uint8_t numplaces){
  assert (numplaces < 8);
  return numplaces == 0 ? byte : (byte << numplaces) | (byte >> (8- numplaces));
}

Block leftTransposeState(Block block, uint8_t offset){
  if (offset == 0){ //Undefined behavior
    return block;
  }
  assert(offset <= 8 && offset > 0); //Undefined behavior
  uint8_t temp = block[0]; //Use msb bit of byte 0 as lsb of byte 7
  for (int i = 0; i < block.size()-1; i++) {
    uint8_t carry = block[i+1] >> (8-offset); //shift msb of next byte into lsb
    block[i] = block[i] << offset | carry; //shift current byte left by offset and or with carryover bit
  }
  block[7] = block[7] << offset | (temp >> (8-offset )); //shift byte 7 and or with msb of byte 0
  return block;
}


std::vector<Block> decryptMessage(std::vector<Block> ciphertext, Block key,
                                  substitutionTableArr inverse){
  std::vector<Block> text;
  Block msgblock = {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};
  for (int i = 0; i < ciphertext.size(); i++){
    msgblock = decryptBlock(ciphertext[i], key, inverse);
    text.push_back(msgblock);
  }
  return text;
}

Block decryptBlock(Block cipherblock, Block key, substitutionTableArr inverse){
  const int NUM_ROUNDS = 16;
  for (int i = 0; i < NUM_ROUNDS; i++){//To decrypt, perform reverse order of encrypting operations:
    cipherblock = rightTransposeState(cipherblock, 1); //1. Bitwise rotate block right by 1 bit
    for (int j = 0 ; j < cipherblock.size(); j++){
      cipherblock[j] = performByteSubstitution(cipherblock[j], inverse[j]); //2. Substitute bytes of block using inverse tables
    }
    for (int j = 0 ; j < cipherblock.size(); j++){
      cipherblock[j] ^= key[j]; //3. XOR bytes of block with encryption key (Inverse of XOR is XOR)
    }
  }
  return cipherblock;
}

Block rightTransposeState(Block block, uint8_t offset){
  if (offset == 0){
    return block;
  }
  assert(offset <= 8 && offset > 0); //bitwise shift by < 0 or > sizeof(uint8_t) yields undefined behavior
  uint8_t temp = block[7];
  for (int i = block.size()-1; i >=1; i--) {//since rotating right, start with byte 7 (rightmost) to prevent overwriting bits
    uint8_t leftinbyte = block[i] >> offset; //right shift over bits remaining from current byte by offset bits
    block[i] = block[i-1] << (8-offset) | leftinbyte; //shift offset bits from preceding block into correct position of current byte
    //(ie left); OR with bits of current byte to get resulting byte's bits
  }
  block[0] = temp << (8-offset) | (block[0] >> offset) ;
  return block;
}

bool blockCipher(std::string pass, std::string message,
                 substitutionTableArr subtables){
  substitutionTableArr inversesubs = inverseSubstitutionTables(subtables);
  std::vector<Block> cipher =
    encryptMessage(message, computeKey(pass), subtables);
  std::cout << "\n\n\n\nEncrypted message:\n\n" << readMessage(cipher, message.size()) << "\n\n\n\n";
  std::cout << "\n\nEnter the correct password to decrypt this encrypted message:\n\n";
  std::string passdecrypt;
  while (passdecrypt.empty()){
    std::getline(std::cin, passdecrypt);
  }
  std::vector<Block> plaintext =
    decryptMessage(cipher, computeKey(passdecrypt), inversesubs);
  std::string decryptedmsg = readMessage(plaintext, message.size());
  std::cout << "\nYour decrypted message:\n\n" << decryptedmsg <<std::endl;
  return testBlockCipher(pass, message, passdecrypt, decryptedmsg);
}

bool testBlockCipher(std::string pass, std::string message,
                     std::string passdecrypt, std::string decryptedmsg){
  return pass == passdecrypt && message == decryptedmsg;
}

void RunBlockCipherEncryption(){
  while (true){
    std::string password, message, keyboardinput;
    std::cout << "Enter your secret message:\n\n";
    while (message.empty()){
      std::getline(std::cin, message);
    }
    std::cout << "Choose a password to encrypt your message with:\n\n";
    while (password.empty()){
      std::getline(std::cin,password);
    }
    substitutionTableArr subtables =
      forwardSubstitutionTables(identityPermutation());
    if (!blockCipher(password, message, subtables)){
      std::cout << "\n\n\n\nIncorrect password. Decryption was unsuccessful\n\n";
      return;
    }
    std::cout << "\n\n\n\nBlock cipher message encryption/decryption successful.\n\n";
    std::cout << "\nEnter 0 to exit or any other character to encrypt another message:\n\n";
    std::cin >> keyboardinput;
    if (keyboardinput == "0"){
      break;
    }
    continue;
  }
}

