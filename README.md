# TaskManagerAPI - Test Tecnico

## Obiettivo

Implementa una Minimal API in .NET 8 che consenta di gestire task multi-tenant, salvando i dati in un file JSON locale, con particolare attenzione alla sicurezza dei dati.

## Funzionalità richieste

- POST /tasks
- GET /tasks (con filtraggio tramite header `X-Tenant-ID`
- Salvataggio dei dati in file JSON

## Funzionalità facoltative

- PUT /tasks/{id}
- Test unitari

## Extra

- Per ogni task crea un record di dati nel nostro applicativo. 

Valorizza i seguenti campi (fieldName):

	1. TASK_ID (String 255)
	2. TASK_DESCRIPTION (String 255)
	3. CREATION DATE (Date)

Al seguente link trovi lo swagger: https://services.paloalto.swiss:10443/api2/swagger/index.html

## Istruzioni

- Puoi modificare la struttura del progetto come preferisci
- Usa solo file system (niente database)
- Inserisci le tue risposte nel file `README.md` alla fine

## Domande finali

1. **Hai riscontrato difficoltà? Dove?**
- Well, to begin with, i had issues with .NET and the framework in general, i never worked with .NET before so i had to get the docs and chatgpt and start learning, still i have experience developing APIRests and backend services and i noticed that most of the concepts and design patterns are the same, so i was able to adapt quickly... although some stuff seems like magic for example that .net is able to resolve specific classes / objects / services just based on types this i found really interesting

- But specifically I had issues implementing the Task Repository ( a singelton as a service i use to manage the JSON file ), since i realized that if we get multiple requests (say 1000 request per second), the file is going to be overwritten, so i had to use a semaphore to lock the file while the server accesses it.

- I also had issues finding the credentials for the DocuWare API i was not able to find them in the swagger docs, so i peaked a bit in the other forks ( i hope its ok ) and i found the credentials in the others candidates code

- Lastly, i lost quite the time doing the tests, since i didn't knew that for each test ( class methods ), xunit creates an instance of the class JUST to run each test, so i had conflicts while testing over the real JSON file, to solve this, what i do now is create a temp. JSON file for each test when a new instance is generated ( in constructor ), so there are no conflicts. This actually took some time since i had to learn how to re-inject a TaskRepository service with a different JSON file when the APITests were being run.

2. **Hai fatto assunzioni? Se sì, quali?**
- When loading the app settings, we first load from the json, but if theres ENV variables, we overwrite the values with these, and then we populate the AppSettings class instance.

- When storing / updating a new task, i load the entire file as an array of TaskItem, operate in memory over it ( insert / upadate.. ), and then i overwrite the original file with the updated in-memory file, thats where the use of semaphores comes in.

- For docuware i created a service that queries the docuware api with a payload from the example i found in the swagger docs.

- I assumed that in .NET when querying async functions, in the case that they are blocking, the thread will be blocked until the function returns, so for the Task Repository i assumed that the semaphore wont block the thread, but it will block the file from being overridden, which for now seems to be the case.

- For all the validation and autentication, i use middlewares, i assumed that this is the way to do it in .NET since its a common practice in most of the frameworks.

- I also assumed that that we may want to block banned tenants from accessing the API, so i added a middleware for that too, that checks if the tenant is in a banned list.

- I used interfaces and models to handle the custom implementations of the services and data handling, i suppose this is a common practice in .NET and other strictly typed languages, since i saw it in the docs and in some examples, so i tought to go along with it and make my own.

- Also i assumed some minor stuff such as config for JSON serializations, and the way to handle the requests and responses.

- Same thing for the HTTP Client, i tought that we can use a custom HTTP Client for the DocuWare API so we can parametrize the base URL, and avoid writting the entire URL in the request.

- To have some minimal security and encription, i force all request to be done over HTTPS, altought we are missing a cert.

- Also, major assumption: The structure of the project. I assumed that having a folder for the API, Common / Shared stuff, Infrastructure ( bussiness logic ), and Models will be a good way to organize the project, since i saw that in the docs and in some examples, but i am not sure if this is the best way to do it, so i would like to hear your feedback on that. ( I left a readme with an overview of the project structure PROJECT.md, and i also left comments in the code, so i hope you can understand my thought process and the logic behind some of the decisions i made )

- I Wanted to try it out and deploy it on my VPS, thats why theres a Dockerfile.

- Theres a new endpoint "/_config" that returns the current configuration of the app ( not public, it still needs a tenant header to be able to access it ), i basically used it to test the ENV variables and the JSON file.

3. **Come miglioreresti il codice se fosse un progetto reale?**
- I think we could create also a test file for the DocuWare Service too if we have more information about the API.

- Going back to Task Repository: I think there could be a better way to handle the file access, i was thinking about it and if we stop the server right while it processes a request, the file will be corrupted or the queued async requests will be lost, but i suppose theres not much we can do with a file. Maybe implementing a messaging system consumer / producer would be a better solution, but i think that would be overkill for this project

- Better logging for sure, the current logs are not very informative nor stored anywhere.

- A better / global error handling around the Routers ( API Maps ), that should return a custom message in case of an error ( file access, docuware api error, etc ), instead of the stack trace.

- ENV Variables / Secrets for ANY configuration or credentials, and containerization when deployed.

- Sswitch the tenant autentication with JWT tokens, and also implement a register / login logic for the tenants

- There should be a way to cover input validation at application level, or is it always done with Dtos in .NET?

- I think we can also add some security headers such as nosniff, cors, and hsts to avoid most of the common attacks, again i dont know if .NET's web framework has this by default, but i think its better if i mention it that if i suppose it has.

- Avoid telling the user any sensitive data in the responses, for example right now, if a tennat tries to modify another's tenant task, it will tell them that that task belongs to antother tenant, which is not a good practice, we should just return a generic error message like "Task not found" or "Unauthorized" or something like that.

- One last think i tought about this morning is to set a rate limit for the requests ( in case this is not covered by an api gateway ), to avoid DDOS attacks.

4. **Hai usato strumenti di supporto (AI, StackOverflow, ecc)? Se sì, come?**
- I used ChatGPT to learn about .NET, i didn't even know how to create a project or a sln solution or do imports. I used it to ask for simple code snippets, examples on how to implement stuff ( the api rest, middlewares, services, header access, etc ) and then i would follow up by asking about the code snippts and cross validate with the docs, as some examples given were pretty outdated.

- The docs as i mentioned, it allowed me to learn what patterns to follow and how to implement the API, and also to learn about the framework in general.

- I also used Copilot, i use it most of the time to avoid writing boilerplate code so i can focus on the actual logic of the project
