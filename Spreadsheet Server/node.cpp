#include "node.h"

using namespace std;

void Node::addDependents(std::string t){

  dependents.insert(t);

}

void Node::addDependees(std::string t){
  dependees.insert(t);

}

void Node::removeDependents(std::string t){

  dependents.erase(t);
}

void Node::removeDependees(std::string t){
  dependees.erase(t);

}

std::set<std::string> Node::getStringDependentsList(){
  return dependents;
}

std::set<std::string> Node::getStringDependeesList(){

  return dependees;

}

void Node::setData(std::string s){

  data = s;

}

std:: string Node::getData(){

  return data;
}
  
