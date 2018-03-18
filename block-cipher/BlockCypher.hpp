//
//  BlockCypher.hpp
//  Assignment1-BlockCypher
//
//  Created by Irene Yeung on 1/14/18.
//  Copyright © 2018 Irene Yeung. All rights reserved.
//

#ifndef BlockCypher_hpp
#define BlockCypher_hpp

#include <stdio.h>
#include <cstdint>
#include <string>
#include <array>
#include <vector>

/*
 Type alias declarations (as outlined in specifications)
 
 64-bit cipher blocksize
 Substitution lookup table represented as a 256 element array
 8 subsitution tables -- distinct permutations to be used for specific bytes
 */
using Block = std::array<uint8_t, 8>;
using substitutionTable = std::array<uint8_t,256>;
using substitutionTableArr = std::array<substitutionTable,8>;


/**
 Generates encryption key from user input password.
 
 @param pass password
 @return a 64-bit encryption key (represented an array of 8 uint_t bytes)
  */
Block computeKey(std::string pass);

/*
Array containing the values 0 through 255 in ascending order. (i.e., identity)
 */
substitutionTable identityPermutation();

/**
 Fisher-Yates shuffling algorithm for generating substitution tables.
 
 @param table to be shuffled
 @return randomly shuffled permutation of table
 */
substitutionTable fisherYatesShuffle(substitutionTable table);

/**
 Methods for generating forward and inverse substitution tables.
 
 @param table used to generate all subsequent substitution tables.
 @return array of 8 substitution lookup tables
 */
substitutionTableArr forwardSubstitutionTables(substitutionTable table);
substitutionTableArr inverseSubstitutionTables(substitutionTableArr forward);

/**
 Byte substitution helper function using a lookup table.
 
 @param byte 
        subtable corresponding substitution table for given byte
 @return lookup value in substitution table
 */
uint8_t performByteSubstitution(uint8_t byte, substitutionTable subtable);

/*
 Helper functions: byte bitwise left/right rotation used as a reference for 
 creating functions for the corresponding
 rotation step in block cipher encryption.
*/

uint8_t rotateRight(uint8_t byte, uint8_t numplaces);
uint8_t rotateLeft (uint8_t byte, uint8_t numplaces);

/**
 Left & Right bitwise rotation helper functions used in block cipher encryption/
 decryption functions.

 @param block Block of 8 uint8_t bytes
        offset number of bits to rotate
 @return left/right transposed block
 */
Block leftTransposeState(Block block, uint8_t offset);
Block rightTransposeState(Block block, uint8_t offset);

/**
 Converts string message to vector of Blocks
 
 @param message message
 @return vector of Blocks
 */
std::vector<Block> translateMessage(std::string message);

/**
 Block encryption helper method.
 
 @param block Block of plaintext
        key encryption key
        subtable substitution tables
 @return encrypted block
 */
Block encryptBlock(Block block, Block key, substitutionTableArr subtable);

/**
 Block cipher encryption method.
 
 @param plaintext message
      key encryption key
      subtable substitution tables
 @return vector of encrypted blocks
 */
std::vector<Block> encryptMessage(std::string plaintext, Block key,
                                  substitutionTableArr forward);

/**
 Block decryption helper method.
 
 @param cipherblock Block 
        key encryption key
        inverse inverse substitution tables
 @return decrypted block
 */
Block decryptBlock(Block cipherblock, Block key, substitutionTableArr inverse);

/**
 Decryption of ciphertext message.
 
 @param ciphertext vector of blocks
        key encryption key (std::array<uint8_t, 8>)
        inverse array of 8 inverse substitution tables.
 @return decrypted message as a vector of Blocks
 */
std::vector<Block> decryptMessage(std::vector<Block> ciphertext, Block key,
                                  substitutionTableArr inverse);

/**
 Debugging function that generates bitset for bytes of a message block 
 and outputs to std::cout stream.
 
 @param byteblock array of bytes (std::array<uint8_t, 8>)
 */
void printBits(Block byteblock);

/**
 Converts message from vector of uint8_t byte Blocks to string.
 
 @param message vector of uint8_t 8-byte Blocks
        messagelength length of original message (handling of messages that are 
        of length n where n % 8 is not 0 (i.e., the final block of message
        will be partially filled, empty bytes will have value of 0x0)).
 @return message as a string
 */
std::string readMessage (std::vector<Block> message, uint8_t messagelength);

/**
 Performs encryption and decryption of message.
 
 @param password
        plaintext original plaintext message
        subtables std::array of 8 substitution tables (one for each byte 
        of block) for substitution step
 @return true if encryption/decryption was successful, false otherwise
 */
bool blockCipher(std::string password, std::string plaintext,
                 substitutionTableArr subtables);

/**
 Encryption/Decryption validation method: password entered to decrypt message
 matches original password and decrypted message matches input message.

 @param pass initial password.
        message original plaintext message.
        passdecrypt decryption password.
        decryptmsg decrypted message.
 @return true if both sets of parameters match, false otherwise.
 */
bool testBlockCipher(std::string pass, std::string message,
                     std::string passdecrypt, std::string decryptedmsg);

/**
 Driver method for executing block cipher encryption program code.
 */
void RunBlockCipherEncryption();

#endif /* BlockCypher_hpp */
