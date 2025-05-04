# Author: Elvi Mihai Sabau Sabau
# Date: 04.05.2025

## Comments:
First of all, i want to thank you for this challenge, i've never programmed in .NET before, but regardless of the outcome of this test i learned a lot, and i got the chance to also review different design patterns implemented in this language.

The project contains some extra features that were not required, such as middleware for tenant validation since i wanted to play around with middlewares and get familiar with in .NET.

In the code i left comments of my tough processes and the logic behind some of the decisions i made, i hope you dont mind.



### Project Structure Overview
Here i outline the structure of the project:

- `src/`
  - `JSD.Api/`
    - `Program.cs`: Configures and starts the web API application
    - `Dtos/`
      - `TaskDto.cs`: Data structure for task transfer
    - `Extensions/`
      - `EndpointExtensions.cs`: Maps API endpoints
      - `ServiceCollectionExtension.cs`: Used for custom services, has only http client service
    - `Middleware/`
      - `TenantValidationMiddleware.cs`: Handles tenant validation in requests
  - `JSD.Common/`
    - `Interfaces/`
      - `ITaskRepository.cs`: Defines the contract for task data storage
    - `Settings/`
      - `AppSettings.cs`: Holds application configuration settings
  - `JSD.Domain/`
    - `TaskItem.cs`: Represents task model / structure
  - `JSD.Infrastructure/`
    - `Repositories/`
      - `JsonTaskRepository.cs`: Implements task storage using a JSON file
    - `Services/`
      - `DocuWareService.cs`: Contains thelogic for interacting with DocuWare API