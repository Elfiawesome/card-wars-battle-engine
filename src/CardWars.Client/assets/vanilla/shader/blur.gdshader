shader_type canvas_item;

// Allows us to read what's behind the UI element
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;

// Parameters to replicate CSS
uniform float blur_amount : hint_range(0.0, 5.0) = 2.5; // Controls the blur
uniform float saturation : hint_range(0.0, 3.0) = 1.6;  // 1.6 replicates saturate(160%)
uniform vec4 tint : source_color = vec4(1.0, 1.0, 1.0, 0.0); // Optional CSS background-color

void fragment() {
	// 1. BLUR: Sample the screen texture using LOD (Level of Detail) to create a blur effect.
	vec4 bg_color = textureLod(screen_texture, SCREEN_UV, blur_amount);
	
	// 2. SATURATION: Convert to grayscale, then mix based on saturation value.
	// Standard luminosity weights for RGB
	float luminance = dot(bg_color.rgb, vec3(0.299, 0.587, 0.114));
	vec3 grayscale = vec3(luminance);
	
	// Mix the grayscale and original color. > 1.0 increases saturation.
	vec3 saturated_color = mix(grayscale, bg_color.rgb, saturation);
	
	// 3. TINT (Optional): Mix the blurred background with a semi-transparent color
	vec3 final_color = mix(saturated_color, tint.rgb, tint.a);
	
	// Output the final pixel
	COLOR = vec4(final_color, 1.0);
}