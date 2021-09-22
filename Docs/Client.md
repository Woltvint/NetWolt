# Client
An instance of the NetWolt.Client
When created automatically tries to connect to the server

## constructor
The constructor accepts 4 arguments

- string address - the address of the server you want to connect to
- int port - port of the server
- bool debug (optional) (default: false) - decides if the client writes debug messages into the console
- bool reconnect (optional) (default: false) - decides if the client will try to reconnect when the connection with the server is lost

example:
```cs
	Client client = new Client("192.168.0.42", 25566);
	Client client = new Client("192.168.0.42", 25566, true);
	Client client = new Client("192.168.0.42", 25566, true, true);
```

## methods

### newCommands()
- returns true if there are any unread commands that the client has received

intended use
```cs
	if (client.newCommands())
	{
		//process commands
	}
```

### nextCommand()
- if there is a command to be read returns the next command in line
- if there is not returns an empty command

intended use
```cs
	Command cmd = client.nextCommand();
```


### sendCommand(Command cmd)
- adds the given command into the queue of commands that will be sent to the server

intended use
```cs
	Command cmd = new Command();
	cmd.addParameter(8);
	cmd.addParameter('B');
	
	client.sendCommand(cmd);
```

## variables

### bool debugLog
- controls if the client writes debug lines to the console
- set by the constructor

### bool reconectTry
- sets if the client will attempt reconnecting after losing the connection with the server
- set by the constructor