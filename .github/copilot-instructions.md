# Copilot Instructions for Roads of the Rim (Continued)

## Mod Overview and Purpose

**Roads of the Rim (Continued)** is an updated version of Loconeko's mod for RimWorld. This mod allows players to construct roads on the world map, enhancing caravan travel by providing faster and more efficient movement routes. It was designed to fill the gap left by the unported RimRoads mod by Jecrell and is compatible with save-games. Additionally, the mod includes a full Russian translation, thanks to contributors Festival and kamikadza13.

## Key Features and Systems

1. **Road Construction Sites:**
   - Initiate road construction on the world map by creating construction sites.
   - Plan your road by selecting tiles sequentially and undo mistakes by clicking on the planned road sections.

2. **Caravan Integration:**
   - Caravans equipped with skilled colonists and resources can build roads.
   - Road building is halted during rest periods or if colonists are incapacitated.

3. **Building Process:**
   - Use colonists with Construction skills to progress the build, with a visible progress bar.
   - Construction follows a specific order; only move forward once a section is finished.
   - High tier roads provide movement advantages through various terrain types.

4. **Resource Management:**
   - Bring resources in parts; full material loads aren't necessary.
   - Benefit from reduced costs when upgrading existing roads.

5. **Allied Help:**
   - Allied factions can assist in road construction.

6. **Cost and Terrain Modifiers:**
   - Road costs vary based on terrain and elevation.
   - Modify these costs in the settings for balance or realism adjustments.

## Coding Patterns and Conventions

- **C#:** Utilize object-oriented programming techniques and adhere to RimWorld's modding patterns.
  - Leverage classes such as `DefModExtension_RotR_RoadDef` and members like `GetCost` to manage road properties and costs.
  - Follow RimWorld-specific best practices for injecting custom functionality.

- **XML:** Use XML definitions to extend and modify game assets.
  - Maintain a clear, organized folder structure for easy mod maintenance and updates.
  - Example XML files include `RotR_CleanBuiltRoads.xml` for map generation and `ResearchProject_RotR.xml` for research projects.

## XML Integration

XML files are critical in defining how roads are implemented and interact with the world. Examples include:

- **Defs\MapGeneration\RotR_CleanBuiltRoads.xml:** Defines steps for cleaning and incorporating built roads into the world generation process.
- **Defs\ResearchProjectDefs\ResearchProject_RotR.xml:** Outlines necessary research for unlocking various road-building technologies.
- **DefModExtension:** Utilize RimWorld's XML extension capabilities to introduce custom settings, such as allowable biomes and terrain modifiers for roads.

## Harmony Patching

Harmony is integral for patching RimWorld's base methods to add or modify behavior without altering the original game code directly.

- **Patch Example:** `Alert_CaravanIdle_GetReport.cs` introduces new behavior for caravan idle alerts using a `Postfix` method.
- Ensure that patches are non-destructive and reversible to maintain compatibility with other mods.

## Suggestions for Copilot

- **Code Suggestions:** Focus on context-aware suggestions that integrate seamlessly with existing C# patterns and RimWorld modding practices.
- **XML Completion:** Provide auto-completion recommendations for common XML tags and attributes within RimWorld's framework.
- **Harmony Integration:** Assist in generating Harmony patches that respect the performance and stability requirements of RimWorld.
- **Debugging Aids:** Suggest fixes or optimizations based on detected patterns or known issues typical in RimWorld modding.

With these guidelines, contributors and developers can effectively utilize GitHub Copilot to enhance their workflow and deliver improved versions of the Roads of the Rim mod.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

