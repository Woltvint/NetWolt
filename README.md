# NetWolt

A simple networking solution for client-server communication

Great for simple games and apps where you dont want to touch the network and just want to send some data and not care about how it gets there

Preatty performant and low overhead in most cases


## Documentation
[Command](./Docs/Command.md)

[Server](./Docs/Server.md)

[Client](./Docs/Client.md)


## Setup
All you need to include for the package to work is the package itself

```cs
using NetWolt;
```

### Server setup

To setup the server all you need to do is to create an instance of the server class and it will automatically start the server on the provided port

```cs
Server server = new Server(<port>);
```

### Client setup

The setup is almost the same for the client, but this time you need to include an address as well. The client will then start connecting to the server 

```cs
Client client = new Client(<address>, <port>);
```

## Usage

### Commands

The basic data unit for sending and receiving data with the package is called a command

A command is just a collection of basic data types

To create a command simply instantiate it and fill it with parameters

```cs
Command cmd = new Command();

cmd.addParameter(<data>);
cmd.setParameter(<possition>, <data>);
```

To read parameters from a command simply use the getParameter method

```cs
Command cmd = new Command();

cmd.addParameter(<data>);

int data = (int)cmd.getParameter(0);
```

### Sending Commands

The functions for sending commands differ from client to server slightly 

For the client you can just use the sendCommand method as shown bellow

```cs
Command cmd = new Command();

cmd.addParameter(<data>);

client.sendCommand(cmd);
```

For the server you need to specify the client you want to send the command to with a clientId integer

```cs
Command cmd = new Command();

cmd.addParameter(<data>);

server.sendCommand(<clientId>,cmd);
```

Or you can send the command to all connected clients

```cs
Command cmd = new Command();

cmd.addParameter(<data>);

server.sendCommandToAll(cmd);
```

## Receiving Commands

To read received commands from the server or client you need to use the nextCommand method

As with the previous method the method is a bit different for the server and the client. The server needs the clientId as an argument.

we can first make sure there are any new commands by using the newCommands method and then poll for received commands with the nextCommand method

```cs
if (server.newCommands(<clientId>))
{
	Command cmd = server.nextCommand(<clientId>);
}

if (client.newCommands())
{
	Command cmd = client.nextCommand();
}
```


