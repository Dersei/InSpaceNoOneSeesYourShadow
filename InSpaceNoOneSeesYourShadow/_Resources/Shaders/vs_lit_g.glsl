#version 440

in vec3 vPosition;


out VertexData {
    vec3 color;
} VertexIn;

uniform mat4 modelview;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;


void main()
{
	gl_Position = modelview * view * model * vec4(vPosition, 1.0);
	VertexIn.color = vec3(0,1,1);
}