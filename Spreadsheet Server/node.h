#ifndef SPREADSHEET_NODEN_H
#define SPREADSHEET_NODE_H

#include <string>
#include <set>


class Node{
 private:

  std::string data;


  std::set<std::string > dependents;

  std::set<std::string > dependees;

 public:

  void addDependents(std::string t);

  void addDependees(std::string t);

  void removeDependents(std::string t);

  void removeDependees(std::string t);

  std::set<std::string> getStringDependentsList();

  std::set<std::string> getStringDependeesList();

  void setData(std::string s);

  std:: string getData();
  




};
#endif
