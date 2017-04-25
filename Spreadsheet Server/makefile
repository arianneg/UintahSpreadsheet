main: Server.cpp
	g++ -std=c++11 Server.cpp Formula.cpp /usr/local/lib/libboost_regex.a dependencygraph.cpp node.cpp  -o server

clean:
	rm -f server *~
formula:	Formula.cpp Formula.h
	g++  -o formula Formula.cpp /usr/local/lib/libboost_regex.a
