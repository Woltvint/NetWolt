# Command
A command is the basic data unit of this package sendable through the network.

A command contains a list of objects called parameters that can be of the following types:

- bool
- byte
- short
- int
- char
- float
- string

## methods

### getParameter(int pos)

- returns the parameter at the position specified
- returns byte 0 if there is no parameter there

intended use

```cs
	int a = (int)cmd.getParameter(0);
	string s = (string)cmd.getParameter(1);
```
Command: \[10|"hello"\]
a: 10
s: "hello"

### addParameter(object parameter)

- adds the parameter to the end of the list
- returns void

intended use

```cs
	cmd.addParameter(10);
	cmd.addParameter('g');
	cmd.addParameter((byte)24);
```
Command: \[10|'g'|(byte) 24\]

### setParameter(int pos, object parameter)

- adds or rewrites the parameter at the given position
- if there is empty space between the parameter and the last parameter in the list it will be filled with (byte) 0
- returns true if it rewrites a previously set parameter

intended use

```cs
	bool a = cmd.setParameter(0, "first");
	cmd.setParameter(1, "second");
	cmd.setParameter(5, 6);
	
	bool b = cmd.setParameter(1, false);
	
```
Command: \["first"|false|(byte) 0|(byte) 0|(byte) 0|6\]
a: false
b: true

### ToString()

- returns the command and its contents as a string
- the string starts with a \[ 
- then there is each parameter with an \| in between them
- and an \] at the end

examples:
\[10|"hello"|false|true|0.5f\]
\[1|2|3|4\]

## variables

### empty

- bool
- only true if no parameter was set or loaded from the network
- returned by the nextCommand function when there are no more commands