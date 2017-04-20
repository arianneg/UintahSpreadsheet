#include <stdio.h>
#include <unistd.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h> 
#include <sys/socket.h>
#include <netinet/in.h>
#include <iostream>

using namespace std;

void do_stuff(int sock);

int main(int argc, char** argv)
{
  int sockfd, newsockfd, pid;
  int serverPort = 2112;

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

  cout << "\nServer started successfully...\n" << endl;

  server_addr.sin_family = AF_INET;                // The family of addresses accepted (IPv4 and IPv6)
  server_addr.sin_addr.s_addr = htons(INADDR_ANY); // Holds the IP address used for the socket (The Server's IP)
  server_addr.sin_port = htons(serverPort);        // Port number for the socket to use

  // Bind the socket to the IP address and port number
  if ((bind(sockfd, (struct sockaddr*)&server_addr, sizeof(server_addr))) < 0)
  {
	cout << "\nError binding connection!\n" << endl;
	return -1;
  }


  size = sizeof(server_addr);
  cout << "Listening for clients..." << endl;

  // Listen for socket connections
  listen(sockfd, 5);

  while (1)
  {
	// Accept a new client
	newsockfd = accept(sockfd, (struct sockaddr *)&server_addr, &size);

	if (newsockfd < 0)
	  cout << "\nError accepting client!\n" << endl;

	// Create a new thread for this client
	pid = fork();

	if (pid < 0)
	  cout << "\nError on fork!\n" << endl;

	if (pid == 0)
	{
	  close(sockfd);
	  do_stuff(newsockfd);
	  exit(0);
	}
	else
	  close(newsockfd);
  }

  close(sockfd);
  return 0;
  
}

void do_stuff(int sock)
{
  int n;
  char buffer[1024];

  // Initialize the buffer and read from the socket
  bzero(buffer, 1024);
  n = read(sock, buffer, 1023);
  if (n < 0)
	cout << "\nError on reading from socket!\n" << endl;
  printf("Message from client: %s\n", buffer);

  n = write(sock, "Hello", 18);

  if (n < 0)
	cout << "\nError writing to socket!\n" << endl;
}
