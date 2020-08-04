# EventBase

This is a protobaby of an eventstore

This is an investigation into what happens if you build out an eventstore idea.

This is *NOT* production ready - it is in memory and (I think) already has race conditions if called from multiple threads.
Appending events should be thread safe accross threads.

Specifically this will happen in a new projection iterating over many events that has an event added to it whilst catching up.
That event I expect will not be seen. 

The solution to this will be to move projections out into a task but i don't care about this problem yet!
