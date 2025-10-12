shader_type canvas_item;

uniform sampler2D tex : source_color;

void fragment() {
    COLOR = texture(tex, UV);
}