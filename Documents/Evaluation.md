# Evaluating the Builder System
Feedback from third party students to evaluate the system’s performance and user experience.

## Methodology
We evaluated our builder system by surveying third-party individuals using the system to perform a task. The survey asked questions pertaining to the user’s experience and efficiency of using the system. The answers to such questions tested multiple components of the system and evaluated how intuitive or difficult these components were to use. It also asked for any feedback they may have on how we can improve each component. This feedback is crucial for providing future students with guidance in areas to continue and improve upon this builder system. Here is a breakdown of the methods used to evaluate the system.

* Number of evaluators: Three

* Types of evaluators: Third-party students in the Computer Science Department (not currently in CSCI 540/440)

* Task performed: The users are tasked with creating two cars. (1) building a car from primitive objects and saving it, and (2) spawning the saved car. 

* Types of questions asked: (1) level of unity experience, (2) scale of intuitiveness per component, (3) challenges per component, (4) improvement suggestions, (5) final thoughts/feedback.

## Results and Analysis
* UI - Inventory & Buttons 
  * Summary – 
    * All of the functionality worked, besides saving prefabs. So it was very intuitive. 
    * TODO: fix formatting; update when prefabs are saved.
  * Specifics – 
    * Fix UI inventory formatting to show the entire words and look more appealing.
    * Update the UI inventory to show the saved prefab instantly when a user saves the prefab after grouping. It does get saved into the Resources folder, but doesn’t show up in the UI until the user ends the world and re-joins, then it will appear in the inventory.

* UI - Keybindings 
  * Summary – 
    * All of the functionality worked, somewhat intuitive, but room for improvement.
    * TODO: Change the actual keys to x, y, and z; move objects with arrows; rotation and scaling should be able to specify a number and modify the object respectively.
  * Specifics – 
    * Being able to grow/shrink objects by a specific amount would be nice, instead of having to hit the key many times to grow/shrink to the desired size.
    * Move objects with arrow keys instead of dragging/dropping.
    * Rotation should allow for all angle rotations instead of just 45 degrees.
    * To grow/shrink in the X, Y, Z dimensions, these should use the X, Y, Z keyboard keys. Currently, for example, to scale in the X dimension, the user has to hit the R key which can be confusing.

* Misc.
  * Specifics – 
    * Group selection (dragging and dropping over an entire area to select all of the objects within that area) instead of selecting one object after the other would be easier when selecting objects to group.
    * Changing the colors of objects.
    * It was fun!



Table 1: Questionnaire and feedback from evaluator 1.

|Question|Answer|
|----|----|
|Do you have experience with Unity?|None|
|Creating Objects - How intuitive was it to select the objects from inventory?|Very intuitive - no confusion.|
|What challenges did you face completing this task (creating objects)? If none, enter N/A.|The words were cut in half and a little hard to read what shapes/objects I was selecting.|
|What can be modified to make this task (creating objects) easier to understand, or easier to complete?|Inventory UI could be improved to look nicer, but works functionally|
|Modifying Objects - How intuitive was it to grow, shrink, rotate, or move the objects?|Very intuitive - no confusion.|
|What challenges did you face completing this task (modifying objects)? If none, enter N/A.|Controls took as second to learn but figured it out after a sec|
|What can be modified to make this task (modifying objects) easier to understand, or easier to complete?|Allowing to move objects or scale objects x amount would have made this easier|
|Grouping Objects - How intuitive was it to group and save objects together?|Very intuitive - no confusion.|
|What challenges did you face completing this task (grouping objects)? If none, enter N/A.|N/A|
|What can be modified to make this task (grouping objects) easier to understand, or easier to complete?|Allowing for group selection (a lot of objects at once) would have made this easier|
|Creating the Pre-Saved Car Object - How intuitive was it to re-create the car?|Could not complete this task.|
|What challenges did you face completing this task (re-creating the car)? If none, enter N/A.|could not save the car|
|What can be modified to make this task (re-creating the car) easier to understand, or easier to complete?|no car to input|
|Is there anything else you can let us know about our builder system?|It was fun! |


Table 2: Questionnaire and feedback from evaluator 2.

|Question|Answer|
|----|----|
|Do you have experience with Unity?|None|
|Creating Objects - How intuitive was it to select the objects from inventory?|Very intuitive - no confusion.|
|What challenges did you face completing this task (creating objects)? If none, enter N/A.|Could not read entire words for choosing shapes. Formatting was wrong.|
|What can be modified to make this task (creating objects) easier to understand, or easier to complete?|Alter formatting to fit the entirety of shape names.|
|Modifying Objects - How intuitive was it to grow, shrink, rotate, or move the objects?|Very intuitive - no confusion.|
|What challenges did you face completing this task (modifying objects)? If none, enter N/A.|N/A|
|What can be modified to make this task (modifying objects) easier to understand, or easier to complete?|I think that moving the objects should be done with arrow keys or some other hot keys instead of clicking a drag. Rotation should allow for all angles instead of just 45 degree rotations.|
|Grouping Objects - How intuitive was it to group and save objects together?|Very intuitive - no confusion.|
|What challenges did you face completing this task (grouping objects)? If none, enter N/A.|N/A|
|What can be modified to make this task (grouping objects) easier to understand, or easier to complete?|Grouping was easy, but I was unable to access the saved prefab afterwards.|
|Creating the Pre-Saved Car Object - How intuitive was it to re-create the car?|Could not complete this task.|
|What challenges did you face completing this task (re-creating the car)? If none, enter N/A.|Couldn't do it|
|What can be modified to make this task (re-creating the car) easier to understand, or easier to complete?|After saving, I should be able to access the saved prefab.|
|Is there anything else you can let us know about our builder system?|You should make it more like Minecraft! I want to interact with the environment and grow trees.|


Table 3: Questionnaire and feedback from evaluator 3.

|Question|Answer|
|----|----|
|Do you have experience with Unity?|None|
|Creating Objects - How intuitive was it to select the objects from inventory?|Very intuitive - no confusion.|
|What challenges did you face completing this task (creating objects)? If none, enter N/A.|Labels for shapes were overlapping making it hard to read|
|What can be modified to make this task (creating objects) easier to understand, or easier to complete?|It made sense to me!|
|Modifying Objects - How intuitive was it to grow, shrink, rotate, or move the objects?|Somewhat intuitive - small confusion.|
|What challenges did you face completing this task (modifying objects)? If none, enter N/A.|N/A|
|What can be modified to make this task (modifying objects) easier to understand, or easier to complete?|It would have been more intuitive if the alterations for the dimension were named with the dimension letter. For example, to alter the X dimension, I was required to use the “R” key but I would have preferred to use the “X” key.|
|Grouping Objects - How intuitive was it to group and save objects together?|Somewhat intuitive - small confusion.|
|What challenges did you face completing this task (grouping objects)? If none, enter N/A.|The object didn’t actually save into the inventory.|
|What can be modified to make this task (grouping objects) easier to understand, or easier to complete?|Everything was intuitive, but the next step is to make the Prefab save successfully into my inventory.|
|Creating the Pre-Saved Car Object - How intuitive was it to re-create the car?|Could not complete this task.|
|What challenges did you face completing this task (re-creating the car)? If none, enter N/A.|Could not save the object.|
|What can be modified to make this task (re-creating the car) easier to understand, or easier to complete?|Make progress towards successful saving of prefabs into inventory. I was unable to do this.|
|Is there anything else you can let us know about our builder system?|I wish I could change the colors! And the appearance of Host.|
