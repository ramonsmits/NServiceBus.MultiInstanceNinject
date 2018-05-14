# NServiceBus.MultiInstanceNinject

Hosting multiple endpoint instances in a single process with Ninject and having shared registrations and singletons

This solution has 2 endpoints, A and B. First, a parent ninject container is used with a registration for `IMyService` as a singleton. Then each endpoint instance gets a child container based on the parent container. Each endpoint has a handler that calls `IMyService`, which is shared between the two instances.
