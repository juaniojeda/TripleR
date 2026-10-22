# TripleR

**A VR recycling game built with Unity and C#.**

Sort waste into the correct containers, build scoring combos, and earn coins through timed play sessions. TripleR is a collaborative game project that combines hands-on VR interaction with a recycling theme.

## Gameplay and systems

- **Waste sorting:** containers check each object's category and award points or penalties.
- **Combo scoring:** consecutive correct classifications increase the score multiplier.
- **Timed sessions:** successful sorting extends the timer, while mistakes cost time.
- **Progression:** persistent coins, best scores, star ratings, and session results.
- **Tutorial NPC:** a state-based character patrols, introduces the tutorial, and returns home, with movement and talking animations.
- **Supporting systems:** object pooling, power-ups, menu interfaces, and Wwise audio integration.

## Technology

| Component | Version / approach |
| --- | --- |
| Unity Editor | **6000.0.71f1** |
| Language | C# |
| Rendering | Universal Render Pipeline |
| VR | Meta XR SDK 203.0.0 and OpenXR |
| Audio | Wwise integration |
| Local persistence | Unity PlayerPrefs |

## My contributions

I contributed to TripleR as part of a team. Examples documented in this repository's history include:

- [Tutorial NPC animation](https://github.com/juaniojeda/TripleR/commit/c8e4ac4fe95eaf6a2be659479a51690d560cb360): movement and talking animation states connected to NPC behavior.
- [Combo scoring and HUD feedback](https://github.com/juaniojeda/TripleR/commit/c18b697e861e87c8d444216579f21792f7c5868a): score calculations, timer, performance results, and interface updates.
- [Persistent coin system](https://github.com/juaniojeda/TripleR/commit/db1fc5b13a39001d01ab5e39a09fe798457f0caf): coin rewards, saved player data, and menu display.

— [Juan Ignacio Ojeda](https://github.com/juaniojeda)

## Explore the code

- [`Classification`](TripleR/Assets/Scripts/Classification): sorting, scoring, combos, timer, and performance evaluation.
- [`IA`](TripleR/Assets/Scripts/IA): tutorial NPC behavior and tutorial interface.
- [`Pooling`](TripleR/Assets/Scripts/Pooling): reusable object lifecycle.
- [`PlayerScore`](TripleR/Assets/Scripts/PlayerScore): player profile and persistent progression.
- [`Menu`](TripleR/Assets/Scripts/Menu): menus and result screens.

## Open the project

1. Clone this repository.
2. In Unity Hub, add the **`TripleR`** folder inside the repository.
3. Open it with **Unity 6000.0.71f1** and allow package imports to complete.
4. Review the Meta XR, OpenXR, and Wwise configuration for your development environment before running or building.

## Credits

TripleR is a team project. The repository's [contributors](https://github.com/juaniojeda/TripleR/graphs/contributors) and [commit history](https://github.com/juaniojeda/TripleR/commits/main/) document contributions, including coauthored work.
