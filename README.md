# Cleanic

Framework for building a business service inspired by [Uncle Bob's "The Clean Architecture"](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html).

## Server interaction pattern
```mermaid
sequenceDiagram
    participant User
    participant AppFacade
    participant CommandBus
    participant CommandAgent
    participant EventStore
    participant EventBus
    participant SagaAgent
    participant ProjectionsAgent
    participant StateStore

    CommandAgent->>CommandBus: Subscribe to commands
    SagaAgent->>EventBus: Subscribe to events
    ProjectionsAgent->>EventBus: Subscribe to events

    User->>+AppFacade: HTTP POST with<br/>command data
    AppFacade->>AppFacade: Auth / Validate
    AppFacade->>CommandBus: Publish command
    AppFacade-->>-User: HTTP POST reponse
    
    CommandBus->>+CommandAgent: Run command handler
    CommandAgent->>EventStore: Load aggregate
    CommandAgent->>CommandAgent: Run command<br/>on aggregate
    CommandAgent->>EventStore: Save aggregate
    EventStore->>EventBus: Publish saved events
    CommandAgent->>-CommandBus: Mark command completed
    
    loop Each published event
        EventBus->>+SagaAgent: Run event handler
        SagaAgent->>SagaAgent: Produce commands<br/>according to<br/>inbound
        SagaAgent->>CommandBus: Publish commands
        SagaAgent->>-EventBus: Mark event handled
    end

    loop Each published event
        EventBus->>+ProjectionsAgent: Run event handler
        ProjectionsAgent->>StateStore: Load projection to be updated
        ProjectionsAgent->>ProjectionsAgent: Apply event to projection
        ProjectionsAgent->>StateStore: Save updated projection
        ProjectionsAgent->>-EventBus: Mark event handled
    end

    User->>+AppFacade: HTTP GET with<br/>query data
    AppFacade->>AppFacade: Auth / Validate
    AppFacade->>StateStore: Load queried projection
    AppFacade->>AppFacade: Subtract data<br/>from projection<br/>according to query
    AppFacade-->>-User: HTTP GET reponse
```

## Assembly relations
```mermaid
classDiagram
    Projections ..> UbiquitousLanguage
    Domain ..> UbiquitousLanguage
    
    ApplicationBase ..> UbiquitousLanguage
    ReadApplication ..> ApplicationBase
    ReadApplication ..> Projections
    WriteApplication ..> ApplicationBase
    WriteApplication ..> Domain
```