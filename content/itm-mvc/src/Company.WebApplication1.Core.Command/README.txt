Command stack

To implement a new command, create a new class extending the BaseCommand. Your implementation goes in RunCommand() in the new class.

To use your command, register it in Startup.cs as a service and inject it into the controller function where you want to use it like so:

public IActionResult About([FromServices] GreatCommand command)
