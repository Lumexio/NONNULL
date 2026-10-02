# Unity 2018.2 (PS Vita Target) Constraints

When working in this Unity project, adhere STRICTLY to the following constraints due to the frozen Unity 2018.2.19f1 SDK for PS Vita:

## C# and Architecture
- **C# 6 Syntax ONLY**: Do not use C# 7+ features (no tuples, no `is Type x` pattern matching, no local functions, no ref returns).
- **Single-Threaded**: Do NOT use the Job System, ECS, or DOTS. Write plain `MonoBehaviour` classes.
- **Legacy API**: Use `FindObjectOfType<T>()` instead of `FindObjectsByType`. `FindObjectsOfType<T>()` is allowed.
- **Networking**: Use `WWW` class instead of `UnityWebRequest` for maximum Vita compatibility.
- **Saving**: Use `PlayerPrefs` or `Application.persistentDataPath` + `JsonUtility`.

## Graphics and Rendering
- **Forward Rendering**: Do NOT use deferred rendering (Vita GPU cannot handle it).
- **No Real-time Shadows**: Use baked lighting or Unlit shaders only.
- **No Post-Processing**: Do NOT use Bloom, HDR, SSAO, or the post-processing stack.
- **No GPU Particles**: Use CPU ParticleSystem only, or sprite-based effects.
- **Textures**: Use ETC2 (or PVRTC) compression.

## UI and Input
- **Legacy UI**: Use `UnityEngine.UI.Text` and `UnityEngine.UI.Image`. Do NOT use TextMeshPro.
- **Legacy Input**: Use `UnityEngine.Input`. Do NOT use the new Input System.

## Performance
- **Target**: 60 FPS.
- **Memory Spikes**: Avoid `SceneManager.LoadScene()` to reset levels. Manually reset variables/GameObjects instead to avoid Vita RAM fragmentation.
