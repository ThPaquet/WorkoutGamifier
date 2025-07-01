<h2>Stack</h2>
- Backend is in C#
- Frontend is in React
- EntityFramework should be used for DB migrations
- Application is Mobile-First, which should be reflected in CSS choices. 
- MSTest is favored for backend unit tests
- Use Jest for frontend testing

<h2>File Structure</h2>
- Styling goes in CSS files, never inline.
- Have seperate C# projects for the different applicative layers (Use onion architecture)
  
<h2>Testability</h2>
- The code you write should be testable, and therefore, granular and minimalistic
- Make sure edge cases are tested

<h2>Dependency Injection</h2>
- All services and business logic should be registered and resolved using dependency injection.
- Avoid static classes for logic that could be tested or mocked.
- Use interfaces to decouple implementations from consumers.

<h2>Error Handling</h2>
- All business logic and services must include robust error handling.
- Always validate input and handle potential exceptions gracefully.
- Return meaningful error messages or exceptions that can be traced and managed.
- Avoid catching exceptions unless you can add value (logging, wrapping, meaningful rethrow).
- Add tests for error conditions and edge cases.
