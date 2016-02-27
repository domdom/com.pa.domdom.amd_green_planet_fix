#version 140

// range_ring_resolve.fs

#ifdef GL_ES
precision mediump float;
#endif

uniform sampler2D GBufferDepth;
uniform sampler2D ldf_result;

uniform vec4 GBufferDepth_size;
uniform vec4 GBufferDepth_range;
uniform vec4 light_diffuse;
uniform mat4 light_transform;

in vec3 v_Forward;

out vec4 out_FragColor;

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void main() 
{


    vec2 screenCoord = gl_FragCoord.xy * GBufferDepth_size.zw;

    // Figure out the world position
    float depth = texture(GBufferDepth, screenCoord).x * GBufferDepth_range.y + GBufferDepth_range.x;
    vec3 fragPos = depth * normalize(v_Forward);

    // Put in light space
    vec4 rawLightPos = light_transform * vec4(fragPos, 1.0);

    // cull
    if (any(greaterThan(abs(rawLightPos.xyz), rawLightPos.www)))
        discard;


    // Read from the distance field results
    float distance = texture(ldf_result, screenCoord).x;

    if (distance > 0.99) discard;

    distance /= 0.99;

    float distance_clamped = clamp(distance, 0.0, 1.0);
    float edge = max(clamp((1.0 - abs(distance_clamped * 2.0 - 1.0)) * 4.0, 0.0, 1.0), distance_clamped * 0.1);

    out_FragColor = vec4(light_diffuse.rgb, light_diffuse.a * edge);
}
