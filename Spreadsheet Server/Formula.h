/*
  A valid Formula object starts with = 
  E.g. =(x1+x2+2)
  This class throws invalid_argument (FormulaFormatException) if the format is invalid 
  You can remove the main in the Formula.cpp. We used it for testing. 
 */


#ifndef FORMULA_H
#define FORMULA_H

#include <string>
#include <vector>


class Formula
{
 private:
  std::string cellname;
  std::string cellcontent;
  std::vector<std::string> tokens;
  
 

 public:
  Formula(std::string formula);
  ~Formula();
  std::vector<std::string> checkTokens(std::string & formula);
  bool checkSyntax(std::vector<std::string> tokenList);
  bool isBalanced (std::vector<std::string> input);
  bool checkNameValidity (std::string name);
  bool isDouble (std::string token);
  std::vector<std::string> getTokens();

};

#endif

