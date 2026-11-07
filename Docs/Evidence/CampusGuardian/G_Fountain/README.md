# Animated campus fountain

12 continuous inward jets, splash droplets and expanding water rings. Existing pool shader retained. Water audio is generated locally, spatial, quiet, with 22m maximum range; adjust waterVolume on IUHFountainEffects.

The installer validates all 216 appended sphere instances before replacing the ivory sculpture mesh. Original mesh asset is preserved. Trimmed sculpture contains 1,364 vertices instead of 112,604. No package or external asset was added.

Use IUH Campus > Concept > Add Animated Fountain in a clean saved campus scene to reinstall. Runtime creates a single FX child; 36 bounded particle systems, no particle collision simulation. Observed 648-660 live particles, audio playing with spatialBlend=1.

Before/after use the same camera (cameras.json). Performance samples are 120 Editor frames per phase with the main gameplay camera, switching the original mesh/effects off versus the trimmed mesh/effects on. This is an approximate Editor observation, not a device/build benchmark or proof of a particular performance gain. See performance.json.

An unrelated Package Manager cancellation message was present in Console; no fountain C# or shader errors were observed.
