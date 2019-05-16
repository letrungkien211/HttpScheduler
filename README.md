# Http Jobs Scheduler

## Motivation
* Create an API to dynamically schedule jobs (tasks, actions) with high precision (100ms).
* Use cases
    * Send scheduled messages
    * Create reminders
    * Schedule actions

## Design
* Make use of sorted set in redis cache for scheduling
## How to use
