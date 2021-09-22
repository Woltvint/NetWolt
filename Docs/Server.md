# Server
An instance of the NetWolt.Server
When created automatically starts a server on the given port

## constructor
The constructor accepts 2 arguments

- int port - port of the server
- bool debug (optional) (default: false) - decides if the server writes debug messages into the console

example:
```cs
	Server server = new Server(25566);
	Server server = new Server(25566, true);
```

## methods

### newCommands(int clientId)
- returns true if there are any unread commands that the server has received from the client specified

intended use
```cs
	if (server.newCommands(10))
	{
		//process commands
	}
```

### nextCommand(int clientId)
- if there is a command to be read from the client returns the next command in line
- if there is not returns an empty command

intended use
```cs
	Command cmd = server.nextCommand(10);
```


### sendCommand(int clientId, Command cmd)
- adds the given command into the queue of commands that will be sent to the client specified

intended use
```cs
	Command cmd = new Command();
	cmd.addParameter(8);
	cmd.addParameter('B');
	
	server.sendCommand(10, cmd);
```

### sendCommandToAll(Command cmd)
- adds the given command into the queue of commands that will be sent for each of the connected clients

intended use
```cs
	Command cmd = new Command();
	cmd.addParameter(8);
	cmd.addParameter('B');
	
	server.sendCommandToAll(cmd);
```

### getAllClients()
- returns a list of ints containing all the clientIds of the connected clients

intended use
```cs
	List<int> clients = server.getAllClients();
```

### getNewClients()
- returns a list of ints containing all the clientIds of the newly connected clients
- the list is wiped after it is read

intended use
```cs
	List<int> newClients = server.getNewClients();
```

### getDisconnectedClients()
- returns a list of ints containing all the clientIds of the newly disconnected clients
- the list is wiped after it is read

intended use
```cs
	List<int> disconectedClients = server.getDisconnectedClients();
```

## variables

### bool debugLog
- controls if the server writes debug lines to the console
- set by the constructor