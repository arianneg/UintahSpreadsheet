#ifndef DEPENDENCYGRAPH_H
#define DEPENDENCYGRAPH_H

#include <string>
#include <map>
#include <vector>
#include "node.h"


class DependencyGraph{


 private:
  std::map<std::string, Node>sorted;
  int size;

 public:

  DependencyGraph();

  int Size();

  int  getNumberOfDependees(std::string s );

  bool HasDependents(std::string s);

  bool HasDependees(std::string s);

  std::set<std::string> GetDependents(std::string s);

  std::set<std::string> GetDependees(std::string);

  void AddDependency(std::string s, std::string t);

  void RemoveDependency(std::string s, std::string t);

  // void ReplaceDependees(std::string s, std::vector<std::string> newDependees);

  //void ReplaceDependents(std::string s, std::vector<std::string> newDependents);

  
  
  

  



};
#endif
