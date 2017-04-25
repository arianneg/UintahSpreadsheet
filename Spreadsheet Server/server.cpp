#include <stdio.h>
#include <unistd.h> 
#include <stdio.h>
#include <unistd.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h> 
#include <sys/socket.h>
#include <netinet/in.h>
#include <iostream>
#include <vector>
#include <map>
#include <string>
#include <sstream>
#include <mutex>
#include <boost/property_tree/xml_parser.hpp>
#include <boost/property_tree/ptree.hpp>
#include <boost/foreach.hpp>
#include <set>
#include <sys/time.h> //FD_SET, FD_ISSET, FD_ZERO macros
#include <arpa/inet.h>    //close 
#include <errno.h>

using namespace std;
std::mutex mtx;

map<string,int> filename;
map<int, map<string,string> > spreadsheet;
map<int, string> clients; // Holds the list of clients currently connected to the server <int, string> -> <clientID, Username>
static int documentID;
static int clientID; // Represents the next available client ID
map<int,int> clientIDToSock;
map<int,set<int> > allOpenedSpreadsheet;
map<int, int> clientIDToDocID;


// Forward Declarations
void testServer();
void do_stuff(int sock);
string fileList();
void split (const string& s, char c,
	    vector<string>& v);
void cell_edit(int sock, vector<string> messageTokens);
void newSpreadsheet(int clientID, int sock, vector<string> messageTokens);
void openSpreadsheet(int clientID, int sock, vector<string> messageTokens);
void saveFile(int sock, vector<string> messageTokens);
void fileRename(int sock, vector<string> messageTokens);
void undo_edit(int sock, vector<string> messageTokens);
void redo_edit(int sock, vector<string> messageTokens);
void showFileList(int sock, vector<string> messageTokens);
void closeSpreadSheet (int clientID, int sock, vector<string> messageTokens);
void broadcast (int doc_ID, string message);


#define TRUE   1 
#define FALSE  0 
#define PORT 2112

int main(int argc, char** argv)
{
  testServer();
  /*int sockfd, newsockfd, pid;
    int serverPort = 2112;
    clientID = -1;
    struct sockaddr_in server_addr;
    socklen_t size;

    // Create a new TCP socket
    sockfd = socket(AF_INET, SOCK_STREAM, 0);

    if (sockfd < 0)
    {
    cout << "\nError establishing the connection!" << endl;
    exit(1);
    }

    // The the values in a buffer to zero
    bzero((char *) &server_addr, sizeof(server_addr));

    server_addr.sin_family = AF_INET;                // The family of addresses accepted (IPv4 and IPv6)
    server_addr.sin_addr.s_addr = htons(INADDR_ANY); // Holds the IP address used for the socket (The Server's IP)
    server_addr.sin_port = htons(serverPort);        // Port number for the socket to use

    // Bind the socket to the IP address and port number
    if ((bind(sockfd, (struct sockaddr*)&server_addr, sizeof(server_addr))) < 0)
    {
    cout << "\nError binding connection!\n" << endl;
    return -1;
    }

    cout << "\nServer started successfully...\n" << endl;

    size = sizeof(server_addr);
    cout << "Listening for clients
    ..." << endl;

    // Listen for socket connections
    listen(sockfd, 5);

    while (1)
    {
    // Accept a new client
    newsockfd = accept(sockfd, (struct sockaddr *)&server_addr, &size);
    cout << "newsockfd: " << newsockfd << endl;

    if (newsockfd < 0)
    cout << "\nError accepting client!\n" << endl;

    // Create a new thread for this client
    pid = fork();

    if (pid < 0)
    cout << "\nError on fork!\n" << endl;

    if (pid == 0)
    {
    //mtx.lock();
    close(sockfd);
    clientID += 1;
    string clientIDStr;
    ostringstream convert;
    convert << clientID;
    clientIDStr = convert.str() + '\n';
    sockToClientID[clientID] = newsockfd;
    for (map<int,int>::iterator it = sockToClientID.begin(); it != sockToClientID.end(); ++it)
    {
    cout << "Client ID: " << it->first << " Client Sock: " << it->second << endl; 
    }
    write(newsockfd, clientIDStr.c_str(), 32);
    do_stuff(newsockfd);
    exit(0);
    //mtx.unlock();
    }
    else
    close(newsockfd);
    }

    close(sockfd);*/
  return 0;
  
}

void testServer()
{
  int opt = TRUE;  
  int master_socket , addrlen , new_socket, 
    max_clients = 30 , activity, i , valread , sd;  
  int max_sd;  
  struct sockaddr_in address;  
        
  char buffer[1025];  //data buffer of 1K 
        
  //set of socket descriptors 
  fd_set readfds;  
        
  clientID = -1;
    
  //initialise all client_socket[] to 0 so not checked 
  for (i = 0; i < max_clients; i++)  
    {  
      clientIDToSock[i] = 0;  
    }  
        
  //create a master socket 
  if( (master_socket = socket(AF_INET , SOCK_STREAM , 0)) == 0)  
    {  
      perror("socket failed");  
      exit(EXIT_FAILURE);  
    }  
    
  //set master socket to allow multiple connections , 
  //this is just a good habit, it will work without this 
  if( setsockopt(master_socket, SOL_SOCKET, SO_REUSEADDR, (char *)&opt, 
		 sizeof(opt)) < 0 )  
    {  
      perror("setsockopt");  
      exit(EXIT_FAILURE);  
    }  
    
  //type of socket created 
  address.sin_family = AF_INET;  
  address.sin_addr.s_addr = INADDR_ANY;  
  address.sin_port = htons( PORT );  
        
  //bind the socket to localhost port 8888 
  if (bind(master_socket, (struct sockaddr *)&address, sizeof(address))<0)  
    {  
      perror("bind failed");  
      exit(EXIT_FAILURE);  
    }  
  printf("Listener on port %d \n", PORT);  
        
  //try to specify maximum of 3 pending connections for the master socket 
  if (listen(master_socket, 3) < 0)  
    {  
      perror("listen");  
      exit(EXIT_FAILURE);  
    }  
        
  //accept the incoming connection 
  addrlen = sizeof(address);  
  puts("Waiting for connections ...");  
        
  while(TRUE)  
    {  
      //clear the socket set 
      FD_ZERO(&readfds);  
    
      //add master socket to set 
      FD_SET(master_socket, &readfds);  
      max_sd = master_socket;  
            
      //add child sockets to set 
      for ( i = 0 ; i < max_clients ; i++)  
        {  
	  //socket descriptor 
	  sd = clientIDToSock[i];  
                
	  //if valid socket descriptor then add to read list 
	  if(sd > 0)  
	    FD_SET( sd , &readfds);  
                
	  //highest file descriptor number, need it for the select function 
	  if(sd > max_sd)  
	    max_sd = sd;  
        }  
    
      //wait for an activity on one of the sockets , timeout is NULL , 
      //so wait indefinitely 
      activity = select( max_sd + 1 , &readfds , NULL , NULL , NULL);  
      
      if ((activity < 0) && (errno!=EINTR))  
        {  
	  printf("select error");  
        }  
            
      //If something happened on the master socket , 
      //then its an incoming connection 
      if (FD_ISSET(master_socket, &readfds))  
        {  
	  if ((new_socket = accept(master_socket, 
				   (struct sockaddr *)&address, (socklen_t*)&addrlen))<0)  
            {  
	      perror("accept");  
	      exit(EXIT_FAILURE);  
            }  
            
	  //inform user of socket number - used in send and receive commands 
	  printf("New connection , socket fd is %d , ip is : %s , port : %d \n" , new_socket , inet_ntoa(address.sin_addr) , ntohs (address.sin_port));  

	  clientID += 1;
	  string clientIDStr;
	  ostringstream convert;
	  convert << clientID;
	  clientIDStr = convert.str() + '\n';
          
	  //send new connection greeting message 
	  if( send(new_socket, clientIDStr.c_str(), strlen(clientIDStr.c_str()), 0) != strlen(clientIDStr.c_str()) )  
            {  
	      perror("send");  
            }  
                
	  puts("Welcome message sent successfully");  
                
	  //add new socket to array of sockets 
	  for (i = 0; i < max_clients; i++)  
            {  
	      //if position is empty 
	      if( clientIDToSock[i] == 0 )  
                {  
		  clientIDToSock[i] = new_socket;  
		  printf("Adding to list of sockets as %d\n" , i);  
                        
		  break;  
                }  
            }  
        }  
            
      //else its some IO operation on some other socket
      for (i = 0; i < max_clients; i++)  
        {  
	  sd = clientIDToSock[i];  

	  bzero(buffer, 1025);
                
	  if (FD_ISSET( sd , &readfds))  
            {  
	      //Check if it was for closing , and also read the 
	      //incoming message 
	      if ((valread = read( sd , buffer, 1024)) == 0)  
                {  
		  //Somebody disconnected , get his details and print 
		  getpeername(sd , (struct sockaddr*)&address , \
			      (socklen_t*)&addrlen);  
		  printf("Host disconnected , ip %s , port %d \n" , 
			 inet_ntoa(address.sin_addr) , ntohs(address.sin_port));  
                        
		  //Close the socket and mark as 0 in list for reuse 
		  close( sd );  
		  clientIDToSock[i] = 0;  
                }  
                    
	      //Echo back the message that came in 
	      else
                {  
		  //set the string terminating NULL byte on the end 
		  //of the data read 
		  //buffer[valread] = '\0';  
		  //send(sd , buffer , strlen(buffer) , 0 );
		  static vector<string> messages;
		  static vector<string> messageTokens;
		  static bool isUserName = true;

		  // Convert the entire buffer into a single string
		  string incomingData(buffer);

		  if (incomingData.find('\n') != string::npos)
		    {
		      printf("Entire message from client: %s\n", buffer);

		      char *tok;
		      tok = strtok(buffer, " \n"); // Split on tab characters and a newline character

		      // Push all tokens from a single message to a vector
		      while (tok != NULL)
			{
			  messages.push_back(tok);
			  tok = strtok(NULL, " \n");
			}

		      for (int i = 0; i < messages.size(); i++)
			{
			  cout << "Message: " << messages[i] << endl;

			  istringstream iss(messages[i]);
			  string token;
			  while (getline(iss, token, '\t'))
			    {
			      messageTokens.push_back(token);
			    }
			}

		      for (int i = 0; i < messageTokens.size(); i++)
			{
			  cout << "Message Token: " << messageTokens[i] << endl;
			}

		      // Get the message type (opcode)
		      int opCode = atoi(messageTokens[0].c_str());

		      // The first thing received from the client should always be the username
		      if (isUserName)
			{
			  opCode = -1;
			  isUserName = false;
			  clients.insert(pair<int, string>(clientID, messageTokens[0]));
			}

		      cout << "OpCode: " << opCode << endl;

		      // Call the appropriate functions based on the opCode
		      switch(opCode)
			{
			case 0:
			  showFileList(sd,messageTokens);
			  break;
			case 1:
			  //n = write(sock, (newFile(incomingData)).c_str(), 1024);
			  newSpreadsheet(clientID, sd, messageTokens);
			  break;
			case 2:
			  //n = write(sock, (openFile(incomingData)).c_str(), 1024);
			  openSpreadsheet(clientID, sd, messageTokens);
			  break;
			case 3:
			  cell_edit(sd, messageTokens);
			  break;
			case 4:
			  undo_edit(sd, messageTokens);
			  break;
			case 5:
			  redo_edit(sd, messageTokens);
			  break;
			case 6:
			  saveFile(sd, messageTokens);
			  break;
			case 7:
			  fileRename(sd, messageTokens);
			  break;
			case 9:
			  closeSpreadSheet (clientID, sd, messageTokens);
			  break;
			default:
			  // Do Nothing
			  break;
			}
		    }

		  messages.clear();
		  messageTokens.clear();
                }  
            }  
        } 
    } 
}

void split(const string& s, char c, vector<string>& v) 
{
  string::size_type i = 0;
  string::size_type j = s.find(c);

  while (j != string::npos) {
    v.push_back(s.substr(i, j-i));
    i = ++j;
    j = s.find(c, j);

    if (j == string::npos)
      v.push_back(s.substr(i, s.length()));
  }
}

// OLD SERVER CODE, KEPT JUST IN CASE
/*void do_stuff(int sock)
  {
  int n;
  char buffer[1024];
  vector<string> messages;
  vector<string> messageTokens;
  bool isUserName = true;

  while (1)
  {
  bzero(buffer, 1024);
  n = read(sock, buffer, 1024);
  if (n < 0)
  cout << "\nError on reading from socket!\n" << endl;

  // Convert the entire buffer into a single string
  string incomingData(buffer);

  if (incomingData.find('\n') != string::npos)
  {
  printf("Entire message from client: %s\n", buffer);

  char *tok;
  tok = strtok(buffer, " \n"); // Split on tab characters and a newline character

  // Push all tokens from a single message to a vector
  while (tok != NULL)
  {
  messages.push_back(tok);
  tok = strtok(NULL, " \n");
  }

  for (int i = 0; i < messages.size(); i++)
  {
  cout << "Message: " << messages[i] << endl;

  istringstream iss(messages[i]);
  string token;
  while (getline(iss, token, '\t'))
  {
  messageTokens.push_back(token);
  }
  }

  for (int i = 0; i < messageTokens.size(); i++)
  {
  cout << "Message Token: " << messageTokens[i] << endl;
  }

  // Get the message type (opcode)
  int opCode = atoi(messageTokens[0].c_str());

  // The first thing received from the client should always be the username
  if (isUserName)
  {
  opCode = -1;
  isUserName = false;
  clients.insert(pair<int, string>(clientID, messageTokens[0]));	
  sockToClientID[sock]=clientID;
  }

  cout << "OpCode: " << opCode << endl;

  // Call the appropriate functions based on the opCode
  switch(opCode)
  {
  case 0:
  showFileList(sock,messageTokens);
  break;
  case 1:
  //n = write(sock, (newFile(incomingData)).c_str(), 1024);
  newSpreadsheet(sock, messageTokens);
  break;
  case 2:
  //n = write(sock, (openFile(incomingData)).c_str(), 1024);
  openSpreadsheet(sock, messageTokens);
  break;
  case 3:
  cell_edit(sock, messageTokens);
  break;
  case 4:
  undo_edit(sock, messageTokens);
  break;
  case 5:
  redo_edit(sock, messageTokens);
  break;
  case 6:
  saveFile(sock, messageTokens);
  break;
  case 7:
  fileRename(sock, messageTokens);
  break;
  case 9:
  //closeSpreadsheet (sock, messageTokens);
  break;
  default:
  // Do Nothing
  break;
  }
  }

  messages.clear();
  messageTokens.clear();
  }
  }*/

// Check if file name is not existed. Then assign new DocID for this file
// if the file exists, send a list of available spreadsheet
void newSpreadsheet(int clientID, int sock, vector<string>messageTokens){
  int n;
  string name = messageTokens[1];
  if (filename.find(name)==filename.end()){
    filename[name]=documentID;
    send(sock,("1\t"+std::to_string(documentID)+"\n").c_str(), strlen(("1\t"+std::to_string(documentID)+"\n").c_str()), 0);
    clientIDToDocID[clientID] = documentID;
    allOpenedSpreadsheet[documentID].insert(clientID);
    documentID++;
  }
  else{
    showFileList(sock, messageTokens);
  }
}

void openSpreadsheet(int clientID, int sock, vector<string>messageTokens){
  int n;
  string name = messageTokens[1];
  map<string,int>::iterator d = filename.find(name);
  
  if (d !=filename.end()){
    int DocID = d->second;
    map<int,map<string,string>>::iterator s = spreadsheet.find(DocID);

    cout << "OPEN AND DOC ID: " << DocID << endl;

    if(s != spreadsheet.end())
      {
	map<string,string> currentSpreadsheet = s->second;
	for(map<string,string>::iterator it=currentSpreadsheet.begin();it!=currentSpreadsheet.end();it++){
	  string cellName = it->first;
	  string cellContent = it->second;
	  send(sock, ("2\t" + std::to_string(DocID) + "\n").c_str(), strlen(("2\t" + std::to_string(DocID) + "\n").c_str()), 0);
	  send(sock, ("3\t"+std::to_string(DocID)+"\t" + cellName + "\t" + cellContent + "\n").c_str(), strlen(("3\t"+std::to_string(DocID)+"\t" + cellName + "\t" + cellContent + "\n").c_str()), 0);
	 
	}
	clientIDToDocID[clientID] = DocID;
	allOpenedSpreadsheet[documentID].insert(clientID);
      }
  }
  else{
    showFileList(sock, messageTokens);
  }
}

// Function that handles cell edits
void cell_edit(int sock, vector<string> messageTokens)
{
  int n;
  int DocID = atoi(messageTokens[1].c_str());
  string cellName = messageTokens[2];
  string newContents = messageTokens[3];

  // Circular Dependency Check Still Required

  // Save Cell Contents for Undo Still Required

  // Save cell name and cell contents
  spreadsheet[DocID][cellName] = newContents;

  for(map<int, int>::iterator it = clientIDToDocID.begin();it!=clientIDToDocID.end();it++)
    {
      if (it->second == DocID)
	{
	  send(clientIDToSock[clientID], ("4\t" + messageTokens[1]+"\n").c_str(), strlen(("4\t" + messageTokens[1]+"\n").c_str()), 0); // Sends a valid message
	  send(clientIDToSock[clientID], ("3\t" + messageTokens[1]+"\t" + cellName + "\t" + newContents + "\n").c_str(), strlen(("3\t" + messageTokens[1]+"\t" + cellName + "\t" + newContents + "\n").c_str()), 0); // Send the cell edit
	}
    }
}

void saveFile(int sock, vector<string>messageTokens){
  int n;
  string name = messageTokens[1];
  map<string,int>::iterator d = filename.find(name);
  if (d !=filename.end()){
    int DocID = d->second;
    send(sock,("7\t"+std::to_string(DocID)+"\n").c_str(), strlen(("7\t"+std::to_string(DocID)+"\n").c_str()), 0);
  }
}

void fileRename(int sock, vector<string>messageTokens){
  int n;
  int DocID = atoi(messageTokens[1].c_str());
  string newFileName = messageTokens[2];

  
  if (filename.find(newFileName)==filename.end()){
    for(map<string,int>::iterator it = filename.begin();it!=filename.end();it++){
      if(it->second == DocID){
	filename.erase(it);
	filename[newFileName] = DocID;
	send(sock, ("6\t"+std::to_string(DocID)+"\t" +newFileName+"\n").c_str(), strlen(("6\t"+std::to_string(DocID)+"\t" +newFileName+"\n").c_str()), 0);
      }
    }
  }
  else{
    send(sock, ("9\t"+std::to_string(DocID)+"\n").c_str(), strlen(("9\t"+std::to_string(DocID)+"\n").c_str()), 0);
  }
}

void undo_edit(int sock, vector<string> messageTokens)
{
  cout << "UNDO CALLED!" << endl;
}

void redo_edit(int sock, vector<string> messageTokens)
{
  cout << "REDO CALLED!" << endl;
}

void showFileList(int sock, vector<string> messageTokens){
  string fileNames="0\t";
  for (map<string,int>::iterator it = filename.begin(); it != filename.end(); ++it)
    {
      string temp = it->first + "\t";
      fileNames.append(temp);
    }
  fileNames.append("\n");
  send(sock, fileNames.c_str(), strlen(fileNames.c_str()), 0);
}

void closeSpreadSheet (int userID, int sock, vector<string> messageTokens)
{
  int doc_ID = atoi (messageTokens[1].c_str());
  set<int> socks=allOpenedSpreadsheet[doc_ID];
  socks.erase(sock);
  string message="A\t"+messageTokens[1]+"\t-1"+"\t"+to_string(userID)+"\t"+clients[userID]+"\n";
  broadcast (doc_ID, message);
  allOpenedSpreadsheet[doc_ID]=socks;
  cout<<message<<endl;
}

void broadcast (int doc_ID, string message){
  set<int> socks=allOpenedSpreadsheet[doc_ID];
  for (set<int>::iterator it = socks.begin() ; it != socks.end(); ++it){
    send(*it, message.c_str(), strlen(message.c_str()), 0);
    cout<<"broadcast message: "<<message<<endl;
  }
}
