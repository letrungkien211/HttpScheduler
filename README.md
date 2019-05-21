# Http Jobs Scheduler

## Motivation
* Create an API to dynamically schedule http jobs (callbacks) in real time
* Use cases
    * Send scheduled messages
    * Create reminders
    * Schedule actions

## Design
* Make use of sorted set in redis cache for scheduling

## How to try
* Build the project with internal redis server
```console
    cd KL.HttpScheduler.Api
    docker-compose build
    docker-compose up
```
* Go to http://localhost:8080/swagger

## Configurable parameters
* Important
    * Config__RedisConnectionString
    * ASPNETCORE_ENVIRONMENT
* For complete list, check the following classes in KL.HttpScheduler.Api
    * Config
    * ApplicationInsightsConfig

## Notes
* To ensure the scalability, make sure the callback endpoint returns as fast as possible. Less than 10ms should be reached. Use fire-and-forget approach if neccessary.

## References