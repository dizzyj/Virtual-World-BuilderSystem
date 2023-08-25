## Builder System

### CSCI 540 Virtual Worlds - Group 5
#### Spring 2023 

David Jenson, Evan Johnson, Ryan Turner, Katie Christensen

The Builder System provides an outlet for user creativity by presenting the tools that allow users to create structures in a persistent (continues to exist even when there are no users present) Virtual World. Through the use of primitive objects, users can stack, move, color, and group to save their creations as prefabs (create and group objects as reusable assets). This functionality allows users to customize their experience in the Virtual World based on their individual presences and needs. The system will provide users with the ability to manipulate shapes, scale, dimensions, and rotation of their 3D space and objects. 

| Key | Task |
| ------ | ------|
| L Shift | Select or deselect objects |
|L Control | Enter shrink mode |
| Q | Rotate the object left |
| E | Rotate the object right |
| R | Scale in the y axes |
| F | Scale in the x axes |
| G | Scale in the z axes |
| T | Expand the object |
| Z | Delete the object (when it is pre-selected) |
| v | Show inventory |

### Current System
* Place pimitives from inventory into the world.
* Modify primitives in the world (position, rotation, scale).
* Clear all placed primitives in the world.
* Group objects in the world - saved to inventory as prefabs.
* Place prefabs from inventory into the world.
* Modify prefabs in the world (position, rotation, scale).
* Clear all prefabs in the world.
* Modify individual objects within prefabs.
* Inventory and spawning limits.
* Persistent inventory with primitives and prefabs. 
* Networking on primitives and prefabs so other players see objects placed and modified.

### Future Work
* Future students should consider implementing persistence and further networking of the objects to allow users to have a better shared experience using the system. One could do this by integrating a database into the system.
* Modifying the control scheme UI to be more intuitive and user friendly.
* Sharing saved prefabs between users. 
* Implementing a permissions system to limit the actions others can take on one's objects.
* Changing color and material of objects.
* Update the inventory to show the newly saved prefab when a user groups objects together. Currently, the user has to save the prefab, exit the world, and then re-open the world and the inventory will then be updated.
* Change the keys x, y, and z when scaling in the x, y and z dimensions.
* Rotation and scaling should be able to specify a number and modify the object respectively.  

### Bios
**[Ryan Turner](https://www.linkedin.com/in/ryan-t-turner/) -** Turner is a senior student in the Computer Science Department at Western Washington University. He has completed relevant courses and is expecting to graduate with a BS degree in Spring 2023.  He has done an internship through Technology Alliance Group Northwest as a Web Developer. He will be responsible for ui interaction and modification.    

**[Katie Christensen](https://www.linkedin.com/in/katie-r-christensen/) -** Christensen is a BS/MS student in the Computer Science Department at Western Washington University. She has completed relevant courses and is expecting to graduate with a BS degree in June, 2023 and a MS in Spring 2024. She was an Undergraduate Intern Trainee at the Institute for Systems Biology in Summer 2022, and is an incoming Digital Notification Delivery Software Engineer Intern at T-Mobile in Summer 2023. She will be responsible for maintaining an extensible project through organization and documentation. She will also be responsible for grouping, saving, and clearing prefabs. 

**[David Jenson](https://www.linkedin.com/in/david-k-jenson/) -** Jenson is a Junior student  in the Computer Science Department at Western Washington University, He has completed relevant courses and is expected to graduate with a BS degree in Spring 2024. Jenson is responsible for migrating the previous builder system to the new virtual world and how prefabs are created and managed by players. 

**[Evan Johnson](https://www.linkedin.com/in/evan-johnson-79b912224/) -** Johnson is a senior student in the Computer Science Department at Western Washington University. He has completed relevant courses and is expected to graduate with a BS degree in Spring 2023. He enjoys working with databases and mobile devices. He will be responsible for working on the UI, modifying objects, and database and prefab implementation.

**Dr. Wesley Deneke, Mentor –** Deneke is a professor in the Computer Science Department.  He leads student research projects that are currently focusing on how to simulate Human Workflows using 3D virtual worlds.
