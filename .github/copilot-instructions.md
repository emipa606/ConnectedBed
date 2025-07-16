# GitHub Copilot Instructions for RimWorld Mod: VNPE_ConnectedBed

## Mod Overview and Purpose

The VNPE_ConnectedBed mod is designed to enhance the functionality of beds in RimWorld by integrating features such as blood transfusion and monitoring of pawns through connected devices. The mod aims to provide a more immersive and interactive medical experience, allowing players to manage the health and well-being of their colonists with greater detail and control.

## Key Features and Systems

- **Connected Bed Mechanics**: The introduction of `Building_ConnectedBed`, which extends from `Building_Bed`, allows beds to interact with connected components for enhanced functionalities.
- **Component Interaction**: The `CompBedConnector` class, attached to beds, facilitates actions like feeding, blood transfusion, and monitoring of pawns.
- **Custom Settings**: The `ConnectedBedSettings` class offers configurable settings to adjust mod operations, ensuring that players can customize their gameplay experience.
- **Pawn Monitoring**: Includes logic for monitoring specific pawn needs like blood levels, ensuring that pawns receive appropriate care in the medical system.

## Coding Patterns and Conventions

- **Class Declaration**: Each class adheres to clear naming conventions, indicating its purpose in the game, such as `Building_ConnectedBed` and `CompBedConnector`.
- **Method Design**: Methods in the code follow C# conventions with CamelCase naming and typically include functionality-driven names like `FeedOccupant` and `TransfuseBlood`.
- **Separation of Concerns**: The mod maintains a clean separation between game logic and UI, with methods related to gameplay separated into components and settings managed in `ConnectedBedSettings`.

## XML Integration

Although not explicitly detailed in the provided summary, RimWorld mods usually include XML for defining new game objects and attributes. For integrating with XML:

- Define new ThingDefs and associated properties in XML files.
- Use the XML to instantiate your classes specified in C#, ensuring consistent interaction with the game engine.

## Harmony Patching

Harmony is a crucial part of modding for RimWorld, enabling changes to the game without altering the original game code. Here's how it's generally integrated into your project:

- **Purpose**: To intercept and alter game methods required for the mod’s functionality.
- **Implementation**: Ensure your code includes the necessary Harmony patches to adjust vanilla methods where necessary, like those affecting bed assignments or pawn health monitoring.

## Suggestions for Copilot

- **Class Enhancements**: Provide suggestions to expand the `CompBedConnector` class with potential new features like automated monitoring alerts or advanced medical procedures.
- **XML Definitions**: Autocomplete XML snippets for ThingDefs that correspond to classes defined in the C# codebase.
- **Harmony Patching**: Suggest potential targets for Harmony patches and include templates or outlines for setting them up.
- **Performance Tips**: Remind developers to include optimizations for methods that handle real-time operations, such as blood transfusion processes.
- **Error Handling**: Propose structures for error handling mechanisms to cope with potential gameplay interruptions or incorrect configurations.

By following these instructions, Copilot can assist in maintaining a robust coding standard for expanding and updating the VNPE_ConnectedBed mod. It will help ensure stability and enhance the user experience through automated and intelligent suggestions.
