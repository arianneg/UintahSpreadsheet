#ifndef DEPENDENCYGRAPH_H
#define DEPENDENCYGRAPH_H

#include <string>
#include <map>
#include <vector>
#include "node.h"
#include <list>

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

  std::set<std::string> GetDirectDependents(std::string t);
  
  std::list<std::string> GetCellsToRecalculate(std::string name);
  
  std::list<std::string> GetCellsToRecalculate(std::set<std::string> names);
  
  void Visit(std::string start, std::string name, std::set<std::string> &visited, std::list<std::string> &changed);




};
#endif
