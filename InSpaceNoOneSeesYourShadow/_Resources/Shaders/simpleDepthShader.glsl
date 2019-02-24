#version 440

in vec3 vPosition;
in vec3 vNormal;
in vec2 texcoord;

uniform mat4 lightSpaceMatrix;
uniform mat4 model;

void
main()
{
    gl_Position = lightSpaceMatrix * model * vec4(vPosition, 1.0);
}