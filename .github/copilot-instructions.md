# GitHub Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose

This mod extends the capabilities of the popular game RimWorld by introducing a comprehensive road construction and maintenance system. It enhances the caravan and world interactions with dynamic road building, caravan-specific functionalities, and detailed construction site management. The mod seeks to enrich the player's strategic options and logistical considerations through infrastructure development.

## Key Features and Systems

- **Caravan Management Enhancements**: Offers new gizmos, alerts, and context-specific interactions for caravans, providing players with more detailed information and ease of management.
- **Construction and Development**: Introduces road construction sites where players can initiate and manage road building projects, including interaction with factions for assistance and progress tracking.
- **Faction Interactions**: Allows players to engage with factions for road building assistance, with a detailed tracking and cooldown system for such interactions.
- **Custom Road Types**: Supports different types of roads with unique properties and cost systems, enhancing travel and strategy dynamics on the world map.

## Coding Patterns and Conventions

- **Static Classes**: Many of the utility functions and handlers are implemented as static classes, aiding in modular design and encapsulation of functionality.
- **Naming Pattern**: A consistent naming scheme is used with the pattern `ClassName_MethodName` which provides clarity about which function relates to which class or system.
- **Inheritance and Use of Base Classes**: Types such as `WorldObject` and `WorldComponent` are extended to introduce new world objects and components, aligning with RimWorld's architecture.
- **Encapsulation**: Internal methods like `SetNext` or `SearchForSettlements` are used to manage complex internal operations clearly and effectively.

## XML Integration

- **Custom XML Parsing**: The mod uses XML files to define in-game resources and settings. `RotR_cost` class exemplifies custom XML parsing to handle specific data structures related to road costs.
- **Def Extensions**: Extends RimWorld's xml `Def` system to handle custom road definitions and associated attributes, using classes such as `DefModExtension_RotR_RoadDef`.

## Harmony Patching

- **HarmonyPatches**: The mod uses Harmony, a popular library for runtime patching, to modify and extend existing game behaviors safely. Patching allows the mod to override or extend functionalities without directly altering the game's original code base.
- **Complex Interactions**: The `HarmonyPatches` class contains patches needed to integrate deeply with game mechanics, ensuring smooth operation with original and modded content.

## Suggestions for Copilot

- **Utilize Class and Method Structure**: When suggesting code completions, structure methods to fit within the existing class architecture, respecting static and instance method usage.
- **Optimize Harmony Integration**: Provide suggested Harmony patches in the context of existing methods to streamline mod compatibility and functionality.
- **Align with Naming Conventions**: Maintain the existing naming conventions (`ClassName_MethodName`) for clarity and consistency across new code suggestions.
- **XML Handling**: Offer code snippets for XML data management that integrate seamlessly with RimWorld's modding framework, ensuring ease of data handling and custom definition usage.

These guidelines aim to assist developers in leveraging GitHub Copilot effectively to enhance this mod further, ensuring consistency, functionality, and maintainability across all aspects of the project.
