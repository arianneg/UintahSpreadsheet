#include "dependencygraph.h"
#include <iostream>
using namespace std;

DependencyGraph::DependencyGraph(){

  size = 0;

}


int DependencyGraph::Size(){


  return size;
}
int DependencyGraph::getNumberOfDependees(string s ){
    return this -> GetDependees(s).size();

}




bool DependencyGraph::HasDependents(string s){

  
  if (sorted.find(s) != sorted.end()){
    if(sorted.find(s)->second.getStringDependentsList().size()>0){
      return true;
    }
    else{
      return false;

    }
  }
  return false;
}



bool DependencyGraph::HasDependees(string s){
  if(sorted.find(s)!= sorted.end()){
    if(sorted.find(s)-> second.getStringDependeesList().size()>0){
      return true;

    }
    else{
      return false;
    }

  }
  return false;
}


  set<string> DependencyGraph::GetDependents(string s){
    if(sorted.find(s)!= sorted.end() ){
      return sorted.find(s)->second.getStringDependentsList();



    }else{
    set<string> empty;
    return empty;}
    
  }
    set<string> DependencyGraph::GetDependees(string s){
      if(sorted.find(s)!= sorted.end()){

	return sorted.find(s)-> second.getStringDependeesList();
      }else{
     set<string>empty;
     return empty;}

    }




















void  DependencyGraph::AddDependency(string s , string t){



  if(sorted.find(s)!=sorted.end() && sorted.find(t)!=sorted.end()){


    Node sNode = sorted.find(s)->second;
   
    Node tNode = sorted.find(t)->second;
  
    set<string> sSet = sNode.getStringDependentsList();
    set<string> tSet = tNode.getStringDependeesList();

    if(sSet.find(t)!= sSet.end() &&tSet.find(t)!= tSet.end()){
      
      
      return;
    }
   
    else{
      sorted.find(s)->second.addDependents(t);
      sorted.find(t)->second.addDependees(s);
      size++;
     

    }
  }
  else if(sorted.find(s) == sorted.end() && sorted.find(t) == sorted.end()){
 
 
    Node sNodeNew ;
    Node tNodeNew;

    sNodeNew.setData(s);
    
   

        if(s == t){
     
      sNodeNew.addDependents(s);
     sNodeNew.addDependees(s);
      pair<string, Node> Spaired;
       Spaired = make_pair(s,sNodeNew);
      sorted.insert(Spaired);
      size++;
     return;

	}

sNodeNew.addDependents(t);
      pair<string, Node> Spaired;
       Spaired = make_pair(s,sNodeNew);
      sorted.insert(Spaired);




    tNodeNew.setData(t);
   set<string> tSet = tNodeNew.getStringDependeesList();
  
    tNodeNew.addDependees(s);
    pair<string, Node> Tpaired;
    Tpaired = make_pair(t,tNodeNew);





    sorted.insert(Tpaired);

    set<string> tSet2 = tNodeNew.getStringDependeesList();
   
    size++;
   
    return;
  }

  else if(sorted.find(s) != sorted.end() && sorted.find(t) == sorted.end()){
     
    sorted.find(s)->second.addDependents(t);
    Node tNodeNew;
    tNodeNew.setData(t);
    tNodeNew.addDependees(s);
    pair<string, Node> Tpaired;
    Tpaired = make_pair(t, tNodeNew);
    sorted.insert(Tpaired);
    size++;
    return;
  }
  else if(sorted.find(t)!= sorted.end() && sorted.find(s) == sorted.end()){
 
    sorted.find(t)->second.addDependees(s);
    Node sNodeNew;
    sNodeNew.setData(s);
    sNodeNew.addDependents(t);
    pair<string, Node> Spaired;
    Spaired = make_pair(s, sNodeNew);
    sorted.insert(Spaired);
    size++;
    return;

  }
  
}



void DependencyGraph::RemoveDependency(string s, string t){

  if(sorted.find(s) != sorted.end() && sorted.find(t) != sorted.end()){
    sorted.find(s)->second.removeDependents(t);
    if(sorted.find(s)->second.getStringDependentsList().size() == 0 && sorted.find(s)->second.getStringDependeesList().size() == 0){
      sorted.erase(s);
      

    }
    sorted.find(t)->second.removeDependees(s);
    if(sorted.find(t)->second.getStringDependentsList().size() ==0 && sorted.find(t)->second.getStringDependeesList().size()== 0 ){

      sorted.erase(t);

    }
    size--;


  }
 }





























set<string> DependencyGraph::GetDirectDependents(string t){

  return GetDependents(t);

}

void DependencyGraph::Visit(string start, string name, set<string> &visited, list<string> &changed){
  //cout<<"here"<<endl;
  //cout<<start<<endl;
  //cout<<name<<endl;

  visited.insert(name);
  set<string> directDependents = GetDirectDependents(name);

  set<string>::iterator it;
  
  for(it = directDependents.begin(); it!= directDependents.end(); it++){
    if(*it == start){
      
      throw 0;

    }else if(visited.find(*it) == visited.end()){
      
      Visit(start, *it, visited, changed);

    }

  }
  changed.push_front(name);
  
  

}
list<string> DependencyGraph::GetCellsToRecalculate(set<string> names){

  list<string> changed;
  set<string> visited;
  
  set<string>::iterator it;
  for(it = names.begin(); it != names.end(); it++){
    if(visited.find(*it) == visited.end()){
      
      Visit(*it, *it, visited, changed);

    }

  }
  return changed;

}

list<string> DependencyGraph::GetCellsToRecalculate(string name){
  
 set<string> names;
  names.insert(name);
  return GetCellsToRecalculate(names);
  
}
  













//void DependencyGraph::ReplaceDependees(string s, vector<string> newDependees){


  
// }
//void DependencyGraph::ReplaceDependents(string s, vector<string> newDependents){

//}
