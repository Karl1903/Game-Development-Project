// Creates a "hard" wave with a hard edge and no smooth lighting like a point light.
// radius: Distance to the Front of the Wave (from wave origin)
// fragDist: Distance to the current Fragment (from wave origin)
// Returns a value between 0 and 1 that determines the visibility of the wave
float getHardWave(float radius, float fragDist, float waveWidth) {
    float fragFrontDist = radius - fragDist; // Distance from the fragment to the front of the wave
    bool isInsideWave = (fragDist < radius) && (fragDist > (radius - waveWidth));

    // Add a fade out gradient to the wave (range 0 to 1)
    fragFrontDist = fragFrontDist / waveWidth;

    // Invert gradient and apply mask
    return (1 - fragFrontDist) * isInsideWave;
}

// Creates a "soft" wave that looks similar to a point light source wave.
// radius: Distance to the Front of the Wave (from wave origin)
// fragDist: Distance to the current Fragment (from wave origin)
// Returns a value between 0 and 1 that determines the visibility of the wave
float getSoftWave(float radius, float fragDist) {
    bool isInsideWave = fragDist < radius;
    float originDist = fragDist / radius; // Distance from wave origin (0 - 1)

    // Invert gradient and apply mask
    return (1 - originDist) * isInsideWave;
}

int isInsideBorder(float radius, float fragDist, float borderWidth) {
    return (fragDist < radius) && (fragDist > (radius - borderWidth));
}
