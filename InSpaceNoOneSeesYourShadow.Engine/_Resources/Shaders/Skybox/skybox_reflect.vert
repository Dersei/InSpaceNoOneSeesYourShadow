#version 440
in vec3 vPosition;
in vec3 aNormal;

out vec3 Normal;
out vec3 Position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 viewmodel;

void main()
{
		Normal = mat3(transpose(inverse(model))) * aNormal;
        Position = (model * vec4(vPosition, 1.0)).xyz;
        gl_Position = viewmodel * vec4(vPosition, 1.0);
}  