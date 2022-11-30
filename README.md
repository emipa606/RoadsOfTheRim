# RoadsOfTheRim


![Image](https://i.imgur.com/buuPQel.png)

Update of Loconekos mod
https://steamcommunity.com/sharedfiles/filedetails/?id=1613783924

- Can be added to save-games
- Russian settings translation added, thanks Festival!
- Full russian translation added, thanks kamikadza13!

![Image](https://i.imgur.com/pufA0kM.png)

	
![Image](https://i.imgur.com/Z4GOv8H.png)

# Roads of the Rim


**RotR - A Rimworld mod allowing road construction on the world map**

# Introduction


I decided to reimplement Jecrell's amazing RimRoads mod since it had not been ported to Rimworld 1.0.

# Features



  - Create Road Construction Sites on the world map
  - Send Caravans with skilled colonists, pack animals, and resources to build the road
  - Enjoy faster movement than the base game on high tier roads, including for all already existing roads (can be switched off in the settings)
  - Part or all of the biome, hills, swamps and season movement difficulty is also cancelled
  - Higher tier roads can be built through impassable terrain
  - Get your allies to help you build



# =====LATEST NEWS=====

**HOTFIX  2020/06/05**
Corrected Caravan text that said resting instead of building
Corrected AISR2G technology text and position
**UPDATE 2020/05/06**


-  Both ISR2Gs are now craftable at a Machining table, cost less, and get their own "Road Equipment" category including when forming caravans
-  ISR2Gs from previous versions of the mod should all be converted
-  Glitter roads can be built on water
-  Better messages for caravans and construction legs
-  Cost in Uranium and Components are not increased by terrain any more


**Please test and report any bugs here**

# Details
 

**Construct**
With RotR, caravans can create a construction site on the world map, players can then select a type of road, click from neighbouring tile to neighbouring tile until they reach their desired goal. If you make a mistake, clicking on a planned section of road will remove it and all sections after it. Clicking on the Construction Site or right-clicking anywhere will finalise the construction.

**Work**
By using resources and colonists with Construction skill, the current section (or leg) can be built. A progress bar is shown on the world map.  Once a leg is finished, the caravan automatically moves on to the next leg. *Note : Road legs must be built in the order they were placed on the world map. *

Having all colonists down, resting during night time, or a moving caravan will prevent work. Similarly, some roads require a certain percentage of the workforce to have a certain construction skill. If only a lower percentage is available, work will be reduced. If no colonist in the caravan has the required skill, work will be impossible. Prisoners don't work.

**Cancel construction**
A site can be removed by selecting it and clicking on the "Cancel Construction" button. A Caravan does not need to be there to do so. All resources spent so far will be lost.

**Building costs modifiers**
Construction costs have two components : base costs that depend on the road and modifiers that depend on the terrain. Roads cost more at higher elevation, starting from 1000m. Hills and swamps also have an effect, which can be checked when clicking on a site. When ISR2G is not used, the total cost can be further reduced from 100% down to 10% in the settings. Anything lower than 100% is probably quite unrealistic, but feel free to use the option.

**Important note**
It is not necessary to bring all resources at once in one caravan. A fraction is enough for a certain amount of work. However, it is necessary to bring each material with you.
*As an example a tenth of the asphalt road: 360 Stone, 60 Steel, 30 Chemfuel is enough material for a tenth of the work (180 work.) *

**Upgrade**
When building a road over an existing one that is slower, 30% of the cost of the existing road will be deducted from the cost of the new road, as long as both roads use that same resource (*e.g. A dirt road will provide 120 work and 36 wood when upgrading to a Stone road*)

** Allies help** 
Allied factions can help building roads

**Impact on movement** 
The roads of RotR do not only provide a customised movement cost multiplier compared to the base game, they also cancel some or all of the movement cost of the terrain.

See the summary below to see what percentage of a specific terrain feature is cancelled, depending on the road type. Note that these effects apply both to generated roads (from the base game) and built roads from RotR.

# Summary
 
[table] 
  [tr]
    [th]Road[/th]
    [th]Movement[/th] 
    [th]Biome[/th] 
    [th]Hills[/th] 
    [th]Winter[/th] 
    [th]Work[/th] 
    [th]Wood[/th] 
    [th]Stone[/th] 
    [th]Steel[/th] 
    [th]Chemfuel[/th] 
    [th]Advanced[/th] 
  [/tr]
  [tr]
    [td]Dirt path[/td]
    [td]75%[/td]
    [td]0%[/td]
    [td]0%[/td]
    [td]0%[/td]
    [td]200[/td]
    [td]0[/td]
    [td]0[/td]
    [td]0[/td]
    [td]0[/td]
    [td]0[/td]
  [/tr]
  [tr]
    [td]Dirt road[/td]
    [td]60%[/td]
    [td]25%[/td]
    [td]20%[/td]
    [td]0%[/td]
    [td]400[/td]
    [td]120[/td]
    [td]0[/td]
    [td]0[/td]
    [td]0[/td]
    [td]0[/td]
  [/tr]
  [tr]
    [td]Stone road[/td]
    [td]50%[/td]
    [td]75%[/td]
    [td]40%[/td]
    [td]0%[/td]
    [td]1200[/td]
    [td]240[/td]
    [td]1800[/td]
    [td]300[/td]
    [td]0[/td]
    [td]0[/td]
  [/tr]
  [tr]
    [td]Asphalt road[/td]
    [td]40%, can build through impassable[/td]
    [td]100%[/td]
    [td]60%[/td]
    [td]20%[/td]
    [td]1800[/td]
    [td]0[/td]
    [td]3600[/td]
    [td]600[/td]
    [td]300[/td]
    [td]0[/td]
  [/tr]
  [tr]
    [td]Glitter road[/td]
    [td]25%, can build through impassable[/td]
    [td]100%[/td]
    [td]100%[/td]
    [td]100%[/td]
    [td]3600[/td]
    [td]0[/td]
    [td]3600[/td]
    [td]900[/td]
    [td]300[/td]
    [td]290 plasteel, 35 Uranium , 20 components, 10 adv. components[/td]
  [/tr]
[/table]

# Following the project and reporting bugs
 
       
Feedback welcome on Steam and in the https://ludeon.com/forums/index.php?topic=47205.0]Forum thread

You can check the code on https://github.com/LocoNeko/RoadsOfTheRim]GitHub, as well as the https://github.com/LocoNeko/RoadsOfTheRim/issues]roadmap and current bugs and issues

**Quick fix for Construction sites stuck after an update : https://github.com/LocoNeko/RoadsOfTheRim/wiki/How-to-remove-construction-sites-stuck-after-mod-upgrade**

**IMPORTANT NOTE BEFORE REPORTING A BUG**
Please make sure you have the latest version by verifying integrity as described in https://support.steampowered.com/kb_article.php?ref=2037-QEUH-3335]THIS POST

It will save a lot of your time (and a little bit of mine ;-) )

![Image](https://i.imgur.com/PwoNOj4.png)



-  See if the the error persists if you just have this mod and its requirements active.
-  If not, try adding your other mods until it happens again.
-  Post your error-log using https://steamcommunity.com/workshop/filedetails/?id=818773962]HugsLib and command Ctrl+F12
-  For best support, please use the Discord-channel for error-reporting.
-  Do not report errors by making a discussion-thread, I get no notification of that.
-  If you have the solution for a problem, please post it to the GitHub repository.



https://steamcommunity.com/sharedfiles/filedetails/changelog/2280318231]Last updated 2022-11-30
