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
cout <<"S: "<<s<<endl;
   cout <<"T: "<<t<<endl;


  if(sorted.find(s)!=sorted.end() && sorted.find(t)!=sorted.end()){

 cout <<"both found"<<endl;
    Node sNode = sorted.find(s)->second;
   
    Node tNode = sorted.find(t)->second;
  
    set<string> sSet = sNode.getStringDependentsList();
    set<string> tSet = tNode.getStringDependeesList();

    if(sSet.find(t)!= sSet.end() &&tSet.find(t)!= tSet.end()){
      
      cout <<"duplicate dependency entered"<<endl;
      return;
    }
    // if(tSet.find(t)!= tSet.end()){
    //   cout<<"found s !!"<<endl;

    // }
    //  if(sorted.find(s)->second.getStringDependentsList().find(t)!= sorted.find(s)->second.getStringDependentsList().end() && sorted.find(t)->second.getStringDependeesList().find(s)!= sorted.find(t)->second.getStringDependentsList().end()){
    //
    //	   cout <<"duplicate dependency entered"<<endl;
    //   return;
    // }
    else{
      sorted.find(s)->second.addDependents(t);
      sorted.find(t)->second.addDependees(s);
      size++;
     

    }
  }
  else if(sorted.find(s) == sorted.end() && sorted.find(t) == sorted.end()){
 cout <<"neither  found"<<endl;
 
    Node sNodeNew ;
    Node tNodeNew;

    sNodeNew.setData(s);
    //  sNodeNew.addDependents(t);
    //  pair<string, Node> Spaired;
    //   Spaired = make_pair(s,sNodeNew);
    //  sorted.insert(Spaired);
   
   

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
    cout<<"size: "<<tSet.size()<<endl;

    tNodeNew.addDependees(s);
    pair<string, Node> Tpaired;
    Tpaired = make_pair(t,tNodeNew);





    sorted.insert(Tpaired);

    set<string> tSet2 = tNodeNew.getStringDependeesList();
    cout<<"size: "<<tSet2.size()<<endl;
    cout<<"sNodeNew"<<sNodeNew.getData()<<endl;
    cout<<"tNodeNew"<<tNodeNew.getData()<<endl;
    size++;
   
    return;
  }

  else if(sorted.find(s) != sorted.end() && sorted.find(t) == sorted.end()){
     cout <<"s found"<<endl;
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
 cout <<"t found"<<endl;
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
//void DependencyGraph::ReplaceDependees(string s, vector<string> newDependees){


  
// }
//void DependencyGraph::ReplaceDependents(string s, vector<string> newDependents){

//}
