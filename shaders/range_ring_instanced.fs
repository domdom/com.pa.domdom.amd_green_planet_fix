#version 140

// ranged_ring_instanced.fs

#ifdef GL_ES
precision mediump float;
#endif

uniform sampler2D GBufferDepth;

uniform vec4 GBufferDepth_size;
uniform vec4 GBufferDepth_range;
uniform vec4 ldf_range;

in vec4 v_CenterRadius;

in vec3 v_Forward;

out vec4 out_FragColor;

void main() 
{
    vec2 screenCoord = gl_FragCoord.xy * ldf_range.z * GBufferDepth_size.zw;

    // Figure out the world position
    float depth = texture(GBufferDepth, screenCoord).x * GBufferDepth_range.y + GBufferDepth_range.x;
    vec3 fragPos = depth * normalize(v_Forward);

    // Perform lighting
    vec3 offset = v_CenterRadius.xyz - fragPos;
    float distance = length(offset);
    if (distance > v_CenterRadius.w)
        discard;

    float edge_scale = clamp(depth / (GBufferDepth_size.x * ldf_range.x), 0.0, 4.0); // magic number clamp

    float adjusted_distance = clamp((v_CenterRadius.w - distance) / edge_scale, 0, 0.99);

    out_FragColor = vec4(adjusted_distance, 1.0, 1.0, 1.0);
}
