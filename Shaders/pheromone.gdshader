shader_type canvas_item;

uniform sampler2D pheromone_map;

void fragment() {
	// Called for every pixel the material is visible on.
	vec4 pheromones = texture(pheromone_map, UV);
	float colony = pheromones.r +pheromones.g;
	float alarm = pheromones.r;
	float returning = pheromones.g;
	float search = pheromones.b;

	vec4 color = vec4(colony +alarm, colony + returning, search, pheromones.a);
	COLOR = color;//color1 +color2 +color3 +color4;
}










