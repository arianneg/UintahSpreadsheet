#include "Formula.h"
#include <string>
#include <exception>
#include <iostream>       // std::cout
#include <stack>          // std::stack
#include <vector>         // std::vector
#include <deque> 
#include <boost/regex.hpp>
using namespace std;

Formula::Formula(string formula)
{
  this->cellname = formula;
  this->tokens = checkTokens(formula);
  if(checkSyntax(this->tokens) == false)
    throw std::invalid_argument("FormulaFormatException");
}

Formula::~Formula()
{
}

vector<string> Formula::checkTokens(string & formula)
{
  vector<string> internal;
  
  string rp = "\\("; //right parenthesis
  string lp = "\\)"; //left parenthesis
  string oper = "[\\+\\-*/]";
  string str = "[a-zA-Z_](?: [a-zA-Z_]|\\d)*";
  string number = "(?:\\d+\\.\\d* | \\d*\\.\\d+ | \\d+ ) (?: [eE][\\+-]?\\d+)?";
  string cell = "[a-zA-Z_](0-9)+";
  string space = "\\s+";
  
  string pattern = "=|\\(|\\)|[\\+\\-*/]|[a-zA-Z_][0-9]+|[a-zA-Z_](?:[a-zA-Z_]|\\d)*|(?:\\d+\\.\\d*|\\d*\\.\\d+|\\d+)(?:[eE][\\+-]?\\d+)?|\\s+";
 
  int match[] = {-1,0};
  boost::regex re (pattern);    
  boost::sregex_token_iterator p (formula.begin(),formula.end(),re,match);
  boost::sregex_token_iterator end;
  while (p!=end){
    string s (*p++);   
    if(s.find_first_not_of(' ') != std::string::npos){
      internal.push_back(s);
    }
  }
  
  return internal;
}
bool Formula::isBalanced (std::vector<std::string> input)
{
  std::stack<std::string> paran; // empty stack
  for (int i=0; i<input.size(); i++){
    if (input[i]=="("){
      paran.push(input[i]);
    }
    else if (input[i]==")"&& !paran.empty()){
      paran.pop();
    }
    else if (input[i]==")"&& paran.empty()){
      return false;
    }
  }
  return (paran.empty());
}

bool Formula::checkNameValidity (std::string name)
{
  if (boost::regex_match (name, boost::regex("^[a-zA-Z_]+[0-9]*")))
    return true;
  else
    return false;
}

bool Formula::isDouble (std:: string token)
{
  if (boost::regex_match (token, boost::regex("(?:\\d+\\.\\d*|\\d*\\.\\d+|\\d+)(?:[eE][\\+-]?\\d+)?")))
    return true;
  else
    return false;
}

bool Formula::checkSyntax(vector<string> tokenList)
{
  if(tokenList.size() == 0 || tokenList.front() != "=")
    {

      return false;
    }
  int countLp = 0; //count left parenthesis
  int countRp = 0; //count right parenthesis
 
  // check the index 1
  
  // iterating from index 2
  for (unsigned i = 1; i < tokenList.size(); i++)
    {
     
      string token(tokenList.at(i));

      if(token == "(") ++countLp;
      if(token == ")") ++countRp;
    
      if(i == 1)
	{
	  if(!(isDouble(token) || checkNameValidity(token) ||token == "("))
	    {

	      return false;
	    }
	}
      string priorToken(tokenList.at(i-1));
      string s = "()+-*/";
      if(!(s.find(token)!=string::npos || checkNameValidity(token) || isDouble(token))){

	return false;
      }
      if(countRp > countLp)
	{

	  return false;
	}
      s="(+-*/";
      if((s.find(priorToken)!=string::npos) && !(isDouble(token)||checkNameValidity(token) || token=="("))
	{

	  return false;

	}
      s=")+-*/";
      if((isDouble(priorToken)||checkNameValidity(priorToken)||priorToken==")") && !(s.find(token)!=string::npos))
	{

	  return false;
	}

    }

  if(countLp != countRp)
    {

      return false;
    }
  string lastToken(tokenList.at(tokenList.size()-1));
  if(!(isDouble(lastToken) || checkNameValidity(lastToken) || lastToken == ")"))
    {

      return false;
    }
  return true;
}



vector<string> Formula::getTokens(){
  return tokens;


}
//int main(){
  
// string formula("=1+x2+2");
// Formula f(formula);
  
  //vector<string> test = f.checkTokens(formula);
  /*
  for(unsigned i = 0; i<test.size();i++){
    cout<<test.at(i)<<endl;
    }*/
  //cout<<f.checkSyntax(test)<<endl;
  
// return 0;

//}
