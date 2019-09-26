#version 440
in vec3 vPosition;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    TexCoords = vPosition;
	gl_Position = projection * view * vec4(vPosition, 1.0);
    //vec4 pos = projection * view * vec4(vPosition, 1.0);
    //gl_Position = pos.xyww;
}  