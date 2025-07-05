Adoption Processor project

The project is fully functional, I created the below projects

1. **Producer API** (ASP.NET Core) - created in the project RabbitAdoption.ProducerAPI
2. **Message Processor** (RabbitMQ + .NET Worker Service) - created in the project RabbitAdoption.WorkerService
   - Set up RabbitMQ message queuing with:

     - Priority queuing (urgent requests processed first)

     - Dead letter handling for failed processing

   - Worker service to:

     - Consume messages from the queue 

     - Apply a matching algorithm (described below) -done by procedure MatchRabbitsToRequest

     - Update request status -done by procedure MatchRabbitsToRequest


3. Due to time constraints, I worked with Ms SQL Server, creating the Adoption database, with the below tables(using EF CORE):
AdoptionRequests - stores the requests for adoption
Rabbits - stores all the rabbits


How I'd extend the solution with more time:
- Implement Logging
- Create a separate project for all the database related data (models, EF core migrations, queries)
- Move to another table(for archive) the rabbits that were already adopted
- periodically try to match the adoption requests that failed initialy

Easter Spike Scenario is handled by:
- using RabbitMQ, which is ideal because it:
Buffers the surge (message queue absorbs burst)
Decouples API from processing logic
- I set up RabbitMQ durable queries to ensures no messages are lost during reboots.
- Set up multiple instances of the .NET Worker
- urgent requests are being prioritezed
- Make a new deployment to return 'Accepted' response to the user instead of processing immediately
