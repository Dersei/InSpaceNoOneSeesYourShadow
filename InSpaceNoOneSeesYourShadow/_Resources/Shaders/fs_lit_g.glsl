#version 440

in vec3  fColor;

out vec4 outputColor;

void main()
{
	 outputColor = vec4(fColor, 0.5);
}