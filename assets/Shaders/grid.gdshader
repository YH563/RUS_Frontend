shader_type spatial;
render_mode blend_mix, depth_draw_opaque, unshaded, cull_disabled;  // 关键: cull_disabled 让背面可见

// 可调节的参数
uniform vec4 gridColor : source_color = vec4(0.3, 0.3, 0.4, 1.0);
uniform vec4 majorColor : source_color = vec4(1.0, 1.0, 1.0, 0.8);
uniform float fadeStart = 5.0;
uniform float fadeEnd = 30.0;
uniform float unitSize = 1.0;
uniform int subdivisions : hint_range(1, 10) = 5;
uniform float majorLineThickness = 1.0;      // 原为2.0，改小
uniform float minorLineThickness = 0.6;      // 原为1.0，改小
uniform float minorLineAlpha : hint_range(0.0, 1.0) = 0.3;
uniform vec3 centerOffset = vec3(0.0, 0.0, 0.0);

float grid(vec2 pos, float unit, float thickness) {
    vec2 threshold = fwidth(pos) * thickness * 0.5 / unit;
    vec2 posWrapped = pos / unit;
    vec2 line = step(fract(-posWrapped), threshold) + step(fract(posWrapped), threshold);
    return max(line.x, line.y);
}

void fragment() {
    vec3 rayWorld = normalize(mat3(INV_VIEW_MATRIX) * VIEW);
    vec3 posWorld;
    float t = -(CAMERA_POSITION_WORLD.y - centerOffset.y) / (rayWorld.y);
    posWorld.y = -centerOffset.y;
    posWorld.xz = CAMERA_POSITION_WORLD.xz + t * rayWorld.xz + centerOffset.xz;

    float distPlanar = distance(posWorld.xz, centerOffset.xz);
    float fadeFactor = 1.0 - clamp((distPlanar - fadeStart) / (fadeEnd - fadeStart), 0.0, 1.0);

    float lineMain = grid(posWorld.xz, unitSize, majorLineThickness);
    float lineSub = grid(posWorld.xz, unitSize / float(subdivisions), minorLineThickness) * minorLineAlpha;
    float line = clamp(lineMain + lineSub, 0.0, 1.0);

    vec2 posAbs = abs(posWorld.xz);
    float epsilon = 0.01;
    bool isAxisX = posAbs.y < epsilon;
    bool isAxisZ = posAbs.x < epsilon;
    vec3 finalColor;
    if (isAxisX || isAxisZ) {
        finalColor = majorColor.rgb;
    } else {
        finalColor = gridColor.rgb;
    }

    vec4 pp = PROJECTION_MATRIX * (VIEW_MATRIX * vec4(posWorld, 1.0));
    DEPTH = pp.z / pp.w;
    ALPHA = line * fadeFactor;
    ALBEDO = finalColor;
}